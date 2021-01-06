
using robotManager.Helpful;
using robotManager.Products;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using wManager.Wow.Enums;
public class Main : wManager.Plugin.IPlugin
{
    private static bool _launched;
    private int _lastReadMessageId = Chat.Messages.Count - 1;
    private readonly List<ChatTypeId> _chatTypeIdFilter = new List<ChatTypeId> { ChatTypeId.WHISPER, ChatTypeId.BN_WHISPER };
    String resultString;
    public void Initialize()
    {


        _launched = true;
        while(_launched)
            Jump();
    }


    public void Dispose()
    {
        _launched = false;
    }

    public void Settings()
    {

    }
    public static bool haveBuff(string spellname)
    {
        return Lua.LuaDoString<bool>(
        @"for i=1,40 do 
            local n = UnitBuff('player', i);
            local m = UnitAura('player', i);
            if n == '" + spellname + @"' or m == '" + spellname + @"' then
                return true;
            end
        end
        return false;
        ");
    }
    void Jump()
    {
        if (haveBuff("Combustion"))
        {
            Move.JumpOrAscend();
        }
        Thread.Sleep(100);
    }


}
