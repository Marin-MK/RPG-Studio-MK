using System;
using System.Collections.Generic;
using static SDL2.SDL;

namespace MKEditor
{
    public enum Keycode
    {
        A = SDL_Keycode.SDLK_a,
        B = SDL_Keycode.SDLK_b,
        C = SDL_Keycode.SDLK_c,
        D = SDL_Keycode.SDLK_d,
        E = SDL_Keycode.SDLK_e,
        F = SDL_Keycode.SDLK_f,
        G = SDL_Keycode.SDLK_g,
        H = SDL_Keycode.SDLK_h,
        I = SDL_Keycode.SDLK_i,
        J = SDL_Keycode.SDLK_j,
        K = SDL_Keycode.SDLK_k,
        L = SDL_Keycode.SDLK_l,
        M = SDL_Keycode.SDLK_m,
        N = SDL_Keycode.SDLK_n,
        O = SDL_Keycode.SDLK_o,
        P = SDL_Keycode.SDLK_p,
        Q = SDL_Keycode.SDLK_q,
        R = SDL_Keycode.SDLK_r,
        S = SDL_Keycode.SDLK_s,
        T = SDL_Keycode.SDLK_t,
        U = SDL_Keycode.SDLK_u,
        V = SDL_Keycode.SDLK_v,
        W = SDL_Keycode.SDLK_w,
        X = SDL_Keycode.SDLK_x,
        Y = SDL_Keycode.SDLK_y,
        Z = SDL_Keycode.SDLK_z,

        N0 = SDL_Keycode.SDLK_0,
        N1 = SDL_Keycode.SDLK_1,
        N2 = SDL_Keycode.SDLK_2,
        N3 = SDL_Keycode.SDLK_3,
        N4 = SDL_Keycode.SDLK_4,
        N5 = SDL_Keycode.SDLK_5,
        N6 = SDL_Keycode.SDLK_6,
        N7 = SDL_Keycode.SDLK_7,
        N8 = SDL_Keycode.SDLK_8,
        N9 = SDL_Keycode.SDLK_9,

        F1 = SDL_Keycode.SDLK_F1,
        F2 = SDL_Keycode.SDLK_F2,
        F3 = SDL_Keycode.SDLK_F3,
        F4 = SDL_Keycode.SDLK_F4,
        F5 = SDL_Keycode.SDLK_F5,
        F6 = SDL_Keycode.SDLK_F6,
        F7 = SDL_Keycode.SDLK_F7,
        F8 = SDL_Keycode.SDLK_F8,
        F9 = SDL_Keycode.SDLK_F9,
        F10 = SDL_Keycode.SDLK_F10,
        F11 = SDL_Keycode.SDLK_F11,
        F12 = SDL_Keycode.SDLK_F12,

        BACKSPACE = SDL_Keycode.SDLK_BACKSPACE,
        RETURN = SDL_Keycode.SDLK_RETURN,
        ENTER = RETURN,
        SPACE = SDL_Keycode.SDLK_SPACE,
        TAB = SDL_Keycode.SDLK_TAB,
        HOME = SDL_Keycode.SDLK_HOME,
        END = SDL_Keycode.SDLK_END,
        INSERT = SDL_Keycode.SDLK_INSERT,
        DELETE = SDL_Keycode.SDLK_DELETE,
        PAGEUP = SDL_Keycode.SDLK_PAGEUP,
        PAGEDOWN = SDL_Keycode.SDLK_PAGEDOWN,
        ESCAPE = SDL_Keycode.SDLK_ESCAPE,

        SCROLLLOCK = SDL_Keycode.SDLK_SCROLLLOCK,
        CAPSLOCK = SDL_Keycode.SDLK_CAPSLOCK,
        NUMLOCK = SDL_Keycode.SDLK_NUMLOCKCLEAR,

        PRINTSCREEN = SDL_Keycode.SDLK_PRINTSCREEN,
        PAUSE = SDL_Keycode.SDLK_PAUSE,

        CTRL = SDL_Keycode.SDLK_LCTRL | SDL_Keycode.SDLK_RCTRL,
        SHIFT = SDL_Keycode.SDLK_LSHIFT | SDL_Keycode.SDLK_RSHIFT,
        ALT = SDL_Keycode.SDLK_LALT | SDL_Keycode.SDLK_RALT,
    }

    public class Key
    {
        public Keycode MainKey;
        public List<Keycode> Modifiers;

        public Key(Keycode MainKey, params Keycode[] Modifiers)
        {
            this.MainKey = MainKey;
            this.Modifiers = new List<Keycode>(Modifiers);
        }
    }
}
