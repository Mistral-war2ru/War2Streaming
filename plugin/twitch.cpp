#include <stdio.h>
#include "patch.h"

#define F_MAP_MSG 0x0042CA40

char pointers[8] = { 0 };
char msg[60] = { 0 };
char bmsg[60] = { 0 };

void show_message(byte time, char* text)
{
    ((void (*)(char*, int, int))F_MAP_MSG)(text, 15, time * 10);//original war2 show msg function
}

PROC g_proc_0045271B;
void update_spells()
{
//this function called every tick when game is played
    ((void (*)())g_proc_0045271B)();//original
    if (msg[0] != 0)
    {
        show_message(20, msg);//20 sec
        msg[0] = 0;
    }
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

extern "C" __declspec(dllexport) void w2p_init()
{
    patch_setdword((DWORD*)pointers, (DWORD)msg);
    patch_setdword((DWORD*)(pointers + 4), (DWORD)bmsg);
    patch_setdword((DWORD*)0x0048FFF0, (DWORD)pointers);

    g_proc_0045271B = patch_call((char*)0x0045271B, (char*)update_spells);
    g_proc_0044CEBB = patch_call((char*)0x0044CEBB, (char*)get_msg);
}

BOOL APIENTRY DllMain(HINSTANCE hInst, DWORD reason, LPVOID reserved) { return TRUE; }
