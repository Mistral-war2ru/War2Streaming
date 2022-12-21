#include <windows.h>
#include <ctype.h>
#include <stdint.h>
#include <stdio.h>
#include "patch.h"//Fois patcher
#include "defs.h"
#include "names.h"
#include "chars.h"

byte reg[4] = { 0 };//region

WORD m_screen_w = 640;
WORD m_screen_h = 480;
WORD m_minx = 176;
WORD m_miny = 16;
WORD m_maxx = 616;
WORD m_maxy = 456;
WORD m_added_height = m_screen_w - 480;
WORD m_added_width = m_screen_h - 640;
WORD m_align_y = 0;
WORD m_align_x = 0;
WORD m_sqx = 14;
WORD m_sqy = 14;

struct unit_names
{
    int name_id;
    int* u;
};
bool game_started = false;
DWORD draw_names = 1;
char pointers[20] = { 0 };
char msg[60];
char bmsg[60];
char users[1000][32];
unit_names namaes[1600];
int last = -1;
byte color = 251;

void show_message(byte time, char* text)
{
    ((void (*)(char*, int, int))F_MAP_MSG)(text, 15, time * 10);//original war2 show msg function
}

void inval_game_view()
{
    ((void (*)(int, int, int, int))F_INVALIDATE)(m_minx, m_miny, m_maxx + 8, m_maxy + 8);
}

bool check_unit_dead(int* p)
{
    bool dead = false;
    if (p)
    {
        if ((*((byte*)((uintptr_t)p + S_FLAGS3))
            & (SF_DEAD | SF_DIEING | SF_UNIT_FREE)) != 0)
            dead = true;
    }
    else
        dead = true;
    return dead;
}

bool check_unit_hidden(int* p)
{
    bool f = false;
    if (p)
    {
        if ((*((byte*)((uintptr_t)p + S_FLAGS3)) & SF_HIDDEN) != 0)//flags3 4 bit
            f = true;
    }
    else
        f = true;
    return f;
}

void set_region(int x1, int y1, int x2, int y2)
{
    byte tmp;
    if (x1 < 0)x1 = 0;
    if (x1 > 127)x1 = 127;
    if (y1 < 0)y1 = 0;
    if (y1 > 127)y1 = 127;
    if (x2 < 0)x2 = 0;
    if (x2 > 127)x2 = 127;
    if (y2 < 0)y2 = 0;
    if (y2 > 127)y2 = 127;
    if (x2 < x1)
    {
        tmp = x1;
        x1 = x2;
        x2 = tmp;
    }
    if (y2 < y1)
    {
        tmp = y1;
        y1 = y2;
        y2 = tmp;
    }
    reg[0] = x1 % 256;
    reg[1] = y1 % 256;
    reg[2] = x2 % 256;
    reg[3] = y2 % 256;
}

bool in_region(byte x, byte y, byte x1, byte y1, byte x2, byte y2)
{
    byte tmp;
    tmp = x % 256;
    x = tmp;
    tmp = y % 256;
    y = tmp;
    tmp = x1 % 256;
    x1 = tmp;
    tmp = y1 % 256;
    y1 = tmp;
    tmp = x2 % 256;
    x2 = tmp;
    tmp = y2 % 256;
    y2 = tmp;
    if (x < 0)x = 0;
    if (x > 127)x = 127;
    if (y < 0)y = 0;
    if (y > 127)y = 127;
    if (x1 < 0)x1 = 0;
    if (x1 > 127)x1 = 127;
    if (y1 < 0)y1 = 0;
    if (y1 > 127)y1 = 127;
    if (x2 < 0)x2 = 0;
    if (x2 > 127)x2 = 127;
    if (y2 < 0)y2 = 0;
    if (y2 > 127)y2 = 127;
    if (x2 < x1)
    {
        tmp = x1;
        x1 = x2;
        x2 = tmp;
    }
    if (y2 < y1)
    {
        tmp = y1;
        y1 = y2;
        y2 = tmp;
    }
    //just check if coords inside region
    return ((x >= x1) && (y >= y1) && (x <= x2) && (y <= y2));
}

PROC g_proc_0045271B;
void update_spells()
{
//this function called every tick when game is played
    game_started = true;
    ((void (*)())g_proc_0045271B)();//original
    if (msg[0] != 0)
    {
        show_message(20, msg);//20 sec
        msg[0] = 0;
    }
    if (draw_names)inval_game_view();
}

PROC g_proc_0044CEBB;
void get_msg(int a, char* text)
{
//this function called when you write in game chat and press enter
    ((void (*)(int, char*))g_proc_0044CEBB)(a, text);//original
    if (text[0] != 0)
    {
        for (int i = 0; i < 60; i++)
        {
            bmsg[i] = text[i];
        }
    }
}

int find_name()
{
    if (last >= 1000)last = 0;
    for (int step = 0; step < 2; step++)
    {
        for (int i = (step == 0) ? last + 1 : 0; i < 1000; i++)
        {
            if (users[i][0] == 0) break;
            else
            {
                bool f = true;//free name
                for (int k = 0; k < 1600; k++)
                {
                    if (namaes[k].name_id == i)f = false;
                }
                if (f) 
                {
                    last = i;
                    return i;
                }
            }
        }
    }
    return -1;
}

int find_place(int* u)
{
    bool f = true;//free name
    int ind = -1;
    for (int k = 0; k < 1600; k++)
    {
        if (namaes[k].u == u)f = false;
        if ((namaes[k].u == NULL) && (ind == -1))ind = k;
    }
    if (!f)return -1;//unit already have name
    return ind;
}

int find_unit_have_name(int* u)
{
    for (int k = 0; k < 1600; k++)if (namaes[k].u == u)return k;
    return -1;
}

PROC g_proc_0040DF71;
int* bld_unit_create(int a1, int a2, int a3, byte a4, int* a5)
{
    int* b = (int*)*(int*)UNIT_RUN_UNIT_POINTER;
    int* u = ((int* (*)(int, int, int, byte, int*))g_proc_0040DF71)(a1, a2, a3, a4, a5);
    if (u != NULL)
    {
        byte o = *((byte*)((uintptr_t)u + S_OWNER));
        if (o == *(byte*)LOCAL_PLAYER)
        {
            int name_id = find_name();
            int pl = find_place(u);
            if ((pl != -1) && (name_id != -1))
            {
                namaes[pl].u = u;
                namaes[pl].name_id = name_id;
            }
        }
    }
    return u;
}

PROC g_proc_00451728;
void unit_kill_deselect(int* u)
{
    int* ud = u;
    ((void (*)(int*))g_proc_00451728)(u);//original
    for (int k = 0; k < 1600; k++)
    {
        if (namaes[k].u == u)
        {
            namaes[k].name_id = -1;
            namaes[k].u = NULL;
            break;
        }
    }
}

PROC g_proc_0044A65C;
int status_get_tbl(void* tbl, WORD str_id)
{
    int* u = (int*)*(int*)UNIT_STATUS_TBL;
    if (u != NULL)
    {
        byte id = *((byte*)((uintptr_t)u + S_ID));
        if (id < U_EMPTY_BUTTONS)
        {
            if (str_id < U_EMPTY_BUTTONS)
            {
                for (int k = 0; k < 1600; k++)
                {
                    if (namaes[k].u == u)
                    {
                        return (int)users + 32 * namaes[k].name_id;
                    }
                }
            }
        }
    }
    return ((int (*)(void*, int))g_proc_0044A65C)(tbl, str_id);//original
}

int* hover_unit;

PROC g_proc_0044AC83;
void unit_hover_get_id(int a, int* b)
{
    if (b != NULL)
    {
        byte id = *((byte*)((uintptr_t)b + 0x20));
        hover_unit = (int*)*(int*)(LOCAL_UNIT_SELECTED_PANEL + 4 * id);
    }
    else
        hover_unit = NULL;
    ((void (*)(int, int*))g_proc_0044AC83)(a, b);//original
}

PROC g_proc_0044AE27;
int unit_hover_get_tbl(void* tbl, WORD str_id)
{
    int* u = hover_unit;
    if (u != NULL)
    {
        byte id = *((byte*)((uintptr_t)u + S_ID));
        if (id < U_EMPTY_BUTTONS)
        {
            if (str_id < U_EMPTY_BUTTONS)
            {
                for (int k = 0; k < 1600; k++)
                {
                    if (namaes[k].u == u)
                    {
                        return (int)users + 32 * namaes[k].name_id;
                    }
                }
            }
        }
    }
    return ((int (*)(void*, int))g_proc_0044AE27)(tbl, str_id);//original
}

PROC g_proc_0042A4A1;
void new_game(int a, int b, long c)
{
    game_started = false;
    for (int i = 0; i < 1600; i++)
    {
        namaes[i].name_id = -1;
        namaes[i].u = NULL;
    }
    ((void (*)(int, int, long))g_proc_0042A4A1)(a, b, c);//original
}

byte* ScreenPTR = NULL;
byte* getScreenPtr() {
    DWORD* r;
    r = (DWORD*)SCREEN_POINTER;
    if (r) { return (byte*)*r; }
    return NULL;
}

int draw_char(int x, int y, byte c, unsigned char ch, byte bc, bool inval)
{
    if (ch < ' ')ch = ' ';
    ch -= ' ';
    int chMfontwidthplus1 = ch * (1 + FONT_6PX_PROP_CHAR_WIDTH);
    int w = 1 + font_6px_prop[chMfontwidthplus1];

    if (x < m_minx)return 0;
    if ((x + w) >= m_maxx)return 0;
    if (y < m_miny)return 0;
    if ((y + 8) > m_maxy)return 0;
    if (!ScreenPTR)return 0;
    byte* p = ScreenPTR + y * m_screen_w + x;
    
    if (!((c == 0) && (bc == 0)))
    {
        if (bc)
            for (int i = 1; i <= w; i++)
            {
                byte font_line = font_6px_prop[chMfontwidthplus1 + i];
                for (int j = 0; j < 8; j++)
                {
                    if (font_line & (1 << j))*p = c;
                    else *p = bc;
                    p += m_screen_w;
                    //                p++;
                }
                p -= m_screen_w * 8;
                p++;
                //            p+=m_screen_w-8;
            }
        else
            for (int i = 0; i < w; i++)
            {
                byte font_line = font_6px_prop[ch * (1 + FONT_6PX_PROP_CHAR_WIDTH) + 1 + i];
                for (int j = 0; j < 8; j++)
                {
                    if (font_line & (1 << j))*p = c;
                    p += m_screen_w;
                    //                p++;
                }
                p -= m_screen_w * 8;
                p++;
                //            p+=m_screen_w-8;
            }
    }
    if (inval)
        ((void (*)(int, int, int, int))F_INVALIDATE)(x, y, x + w, y + 8);
    return w;
}

int draw_text(int x, int y, byte c, unsigned char* ch, byte bc, byte cond, bool inval)
{
    int w = 0;
    int C = 0;
    while (ch[C] != 0)
    {
        w += draw_char(x + w, y, c, ch[C], bc, false);
        w -= cond;
        if (w < 0)w = 0;
        C++;
    }
    if (inval)
        ((void (*)(int, int, int, int))F_INVALIDATE)(x, y, x + w, y + 8);
    return w;
}

void drawing()
{
    ScreenPTR = getScreenPtr();
    if ((*(byte*)GAME_MODE == 3) && (game_started))
    {
        byte cx = *(byte*)CAMERA_X;
        byte cy = *(byte*)CAMERA_Y;
        set_region(cx, cy, cx + m_sqx - 1, cy + m_sqy);
        int* p = (int*)(UNITS_LISTS + 4 * *(byte*)LOCAL_PLAYER);//pointer to units list for player
        if (p)
        {
            p = (int*)(*p);
            while (p)
            {
                if (*((byte*)((uintptr_t)p + S_ID)) < U_FARM)
                {
                    if (!check_unit_dead(p) && !check_unit_hidden(p))
                    {
                        byte x = *((byte*)((uintptr_t)p + S_X));
                        byte y = *((byte*)((uintptr_t)p + S_Y));
                        WORD xx = *((WORD*)((uintptr_t)p + S_DRAW_X));
                        WORD yy = *((WORD*)((uintptr_t)p + S_DRAW_Y));
                        if (in_region((byte)x, (byte)y, reg[0], reg[1], reg[2], reg[3]))
                        {
                            int uid = find_unit_have_name(p);
                            if (uid != -1)
                            {
                                if (users[namaes[uid].name_id][0] != 0)
                                {
                                    draw_text(
                                        m_minx + xx - cx * 32,
                                        m_miny + yy - cy * 32 - 10,
                                        color, (unsigned char*)users[namaes[uid].name_id], 0, 0, true);
                                }
                                else
                                {
                                    namaes[uid].u = NULL;
                                    namaes[uid].name_id = -1;
                                }
                            }
                        }
                    }
                }
                p = (int*)(*((int*)((uintptr_t)p + S_NEXT_UNIT_POINTER)));
            }
        }
    }
}

PROC g_proc_00421F57;
void draw_hook3()
{
    ((void (*)())g_proc_00421F57)();//original
    if (draw_names)drawing();
}

void hook(int adr, PROC* p, char* func)
{
    *p = patch_call((char*)adr, func);
}

extern "C" __declspec(dllexport) void w2p_init()
{
    m_screen_w = *(WORD*)SCREEN_SIZE_W;
    m_screen_h = *(WORD*)SCREEN_SIZE_H;
    m_added_width = m_screen_w - 640;
    m_added_height = m_screen_h - 480;
    m_align_x = m_added_width > 0 ? m_added_width / 2 : 0;
    m_align_y = m_added_height > 0 ? m_added_height / 2 : 0;
    m_maxx = m_minx + m_screen_w - 200;
    m_maxy = m_miny + m_screen_h - 40;
    m_sqx += m_added_width / 32;
    m_sqy += m_added_height / 32;

    memset(msg, 0, sizeof(msg));
    memset(bmsg, 0, sizeof(bmsg));
    memset(users, 0, sizeof(users));
    for (int i = 0; i < 1600; i++)
    {
        namaes[i].name_id = -1;
        namaes[i].u = NULL;
    }
    patch_setdword((DWORD*)pointers, (DWORD)msg);
    patch_setdword((DWORD*)(pointers + 4), (DWORD)bmsg);
    patch_setdword((DWORD*)(pointers + 8), (DWORD)users);
    patch_setdword((DWORD*)(pointers + 12), (DWORD)&draw_names);
    patch_setdword((DWORD*)(pointers + 16), (DWORD)&color);
    patch_setdword((DWORD*)0x0048FFF0, (DWORD)pointers);

    hook(0x0045271B, &g_proc_0045271B, (char*)update_spells);
    hook(0x0044CEBB, &g_proc_0044CEBB, (char*)get_msg);
    hook(0x0040DF71, &g_proc_0040DF71, (char*)bld_unit_create);
    hook(0x00451728, &g_proc_00451728, (char*)unit_kill_deselect);
    hook(0x0044A65C, &g_proc_0044A65C, (char*)status_get_tbl);
    hook(0x0044AC83, &g_proc_0044AC83, (char*)unit_hover_get_id);
    hook(0x0044AE27, &g_proc_0044AE27, (char*)unit_hover_get_tbl);
    hook(0x0042A4A1, &g_proc_0042A4A1, (char*)new_game);
    hook(0x00421F57, &g_proc_00421F57, (char*)draw_hook3);
}

BOOL WINAPI DllMain(HINSTANCE hinstDLL, DWORD ul_reason_for_call, LPVOID) { return TRUE; }
