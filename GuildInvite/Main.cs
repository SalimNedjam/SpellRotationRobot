
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
        Logging.Write("[GuildInvite] Loadded.");
        UpdateReply();
        Logging.Write("[GuildInvite] Disposed.");
    }


    public void Dispose()
    {
        _launched = false;

        Logging.Write("[MoveDuringCombat] Disposed.");

    }

    public void Settings()
    {

    }
    public String getOne()
    {
        return Lua.LuaDoString<String>(@"RunMacroText('/who algerian')
                                        if WhoFrameWhoButton then WhoFrameWhoButton:Click() end
                                        local n=GetNumWhoResults();
                                        local i=1;
                                        local found = false;
                                        while(i<=n) do 
	                                        local c,g=GetWhoInfo(i); 
                                            found = false;
	                                        if(g=='THE ALGERIAN TERRITORY') then
                                                for i=1,GetNumGuildMembers() do
                                                    local name, rank = GetGuildRosterInfo(i)
                                                    if name and found == false then
                                                        name = name:gsub('-Sethraliss', '')
                                                        if name == c then
                                                            found = true;
                                                        end
                                                    end
                                                end
                                                if found == false then
                                                    return c;
                                                end
	                                        end
	                                        i=i+1;
                                        end
                                        return '';
                                        ");
    }

    void UpdateReply()
    {
        do
        {
            resultString = getOne();
            if (!resultString.Equals(""))
            {
                Logging.Write("Reinvite " + resultString);
                Lua.LuaDoString(@"GuildUninvite('" + resultString + "')");
                Thread.Sleep(2000);
                Lua.LuaDoString(@"GuildInvite('" + resultString + "')");

            }

        } while (!resultString.Equals(""));

    }


    //void UpdateReply()
    //{
    //    var msgs = Chat.Messages;
    //    while (_launched)
    //    {
    //        while (_lastReadMessageId + 1 <= msgs.Count - 1)
    //        {
    //            _lastReadMessageId++;

    //            if (_chatTypeIdFilter.Contains(msgs[_lastReadMessageId].Channel))
    //            {
    //                if (msgs[_lastReadMessageId].Msg.Contains("+"))
    //                {
    //                    resultString = System.Text.RegularExpressions.Regex.Match(msgs[_lastReadMessageId].Msg, @"\d+").Value;
    //                    Logging.Write("Reinvite " + msgs[_lastReadMessageId].UserName);
    //                    Lua.LuaDoString(@"GuildUninvite('" + msgs[_lastReadMessageId].UserName + "')");
    //                    Thread.Sleep(1000);
    //                    Lua.LuaDoString(@"GuildInvite('" + msgs[_lastReadMessageId].UserName + "')");
    //                }

    //            }
    //        }
    //        Thread.Sleep(100);
    //    }

    //}

}
