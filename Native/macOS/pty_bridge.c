#include <errno.h>
#include <fcntl.h>
#include <signal.h>
#include <spawn.h>
#include <stdbool.h>
#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <sys/ioctl.h>
#include <sys/types.h>
#include <sys/wait.h>
#include <termios.h>
#include <unistd.h>
#include <util.h>

#define MAX_SESSIONS 32

typedef struct {
    bool used;
    int master_fd;
    pid_t child_pid;
} pty_session;

static pty_session g_sessions[MAX_SESSIONS];

static int alloc_handle(void) {
    for (int i = 1; i < MAX_SESSIONS; i++) {
        if (!g_sessions[i].used) {
            g_sessions[i].used = true;
            g_sessions[i].master_fd = -1;
            g_sessions[i].child_pid = -1;
            return i;
        }
    }
    return -1;
}

static pty_session* get_session(int handle) {
    if (handle <= 0 || handle >= MAX_SESSIONS || !g_sessions[handle].used) {
        return NULL;
    }
    return &g_sessions[handle];
}

static void release_session(int handle) {
    if (handle <= 0 || handle >= MAX_SESSIONS) {
        return;
    }

    g_sessions[handle].used = false;
    g_sessions[handle].master_fd = -1;
    g_sessions[handle].child_pid = -1;
}

int pty_spawn(const char* shell_path, const char* cwd, int cols, int rows) {
    int master_fd = -1;
    int slave_fd = -1;

    struct winsize ws;
    memset(&ws, 0, sizeof(ws));
    ws.ws_col = cols > 0 ? cols : 120;
    ws.ws_row = rows > 0 ? rows : 40;

    if (openpty(&master_fd, &slave_fd, NULL, NULL, &ws) != 0) {
        return -1;
    }

    int handle = alloc_handle();
    if (handle < 0) {
        close(master_fd);
        close(slave_fd);
        return -1;
    }

    pid_t pid = fork();
    if (pid < 0) {
        close(master_fd);
        close(slave_fd);
        release_session(handle);
        return -1;
    }

    if (pid == 0) {
        setsid();
        ioctl(slave_fd, TIOCSCTTY, 0);

        dup2(slave_fd, STDIN_FILENO);
        dup2(slave_fd, STDOUT_FILENO);
        dup2(slave_fd, STDERR_FILENO);

        close(master_fd);
        close(slave_fd);

        if (cwd != NULL && strlen(cwd) > 0) {
            chdir(cwd);
        }

        setenv("TERM", "xterm-256color", 1);

        const char* shell = (shell_path != NULL && strlen(shell_path) > 0) ? shell_path : "/bin/zsh";
        execl(shell, shell, "-l", NULL);
        _exit(127);
    }

    close(slave_fd);

    int flags = fcntl(master_fd, F_GETFL, 0);
    fcntl(master_fd, F_SETFL, flags | O_NONBLOCK);

    g_sessions[handle].master_fd = master_fd;
    g_sessions[handle].child_pid = pid;

    return handle;
}

int pty_write(int handle, const uint8_t* bytes, int len) {
    pty_session* s = get_session(handle);
    if (s == NULL || bytes == NULL || len <= 0) {
        return -1;
    }

    ssize_t w = write(s->master_fd, bytes, (size_t)len);
    return (int)w;
}

int pty_read(int handle, uint8_t* out_buf, int cap) {
    pty_session* s = get_session(handle);
    if (s == NULL || out_buf == NULL || cap <= 0) {
        return -1;
    }

    ssize_t n = read(s->master_fd, out_buf, (size_t)cap);
    if (n > 0) {
        return (int)n;
    }

    if (n == 0) {
        return 0;
    }

    if (errno == EAGAIN || errno == EWOULDBLOCK) {
        return -1;
    }

    if (errno == EIO) {
        return 0;
    }

    return -1;
}

int pty_resize(int handle, int cols, int rows) {
    pty_session* s = get_session(handle);
    if (s == NULL) {
        return -1;
    }

    struct winsize ws;
    memset(&ws, 0, sizeof(ws));
    ws.ws_col = cols > 0 ? cols : 120;
    ws.ws_row = rows > 0 ? rows : 40;

    if (ioctl(s->master_fd, TIOCSWINSZ, &ws) != 0) {
        return -1;
    }

    kill(s->child_pid, SIGWINCH);
    return 0;
}

int pty_kill(int handle) {
    pty_session* s = get_session(handle);
    if (s == NULL) {
        return -1;
    }

    if (s->child_pid > 0) {
        kill(s->child_pid, SIGTERM);
    }

    return 0;
}

int pty_close(int handle) {
    pty_session* s = get_session(handle);
    if (s == NULL) {
        return -1;
    }

    if (s->master_fd >= 0) {
        close(s->master_fd);
        s->master_fd = -1;
    }

    if (s->child_pid > 0) {
        int status = 0;
        waitpid(s->child_pid, &status, WNOHANG);
    }

    release_session(handle);
    return 0;
}
