using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using Decal.Adapter;

namespace ACKillXP
{
    [FriendlyName("AC Kill XP")]
    [System.Runtime.InteropServices.Guid("4EDC248A-E237-4516-A17C-5099F515ADC9")]
    public sealed class PluginCore : PluginBase
    {
        private const string Version = "0.1.0";
        private const int PairWindowMilliseconds = 2500;

        private bool _enabled = true;
        private bool _debug = false;
        private bool _loggedIn = false;
        private long _lastTotalXp = 0;

        private readonly List<KillMarker> _pendingKills = new List<KillMarker>();
        private readonly List<XpGain> _recentXp = new List<XpGain>();

        private static readonly Regex[] KillPatterns = new Regex[]
        {
            new Regex(@"^You flatten .+'s body with the force of your assault!$", RegexOptions.Compiled),
            new Regex(@"^You bring .+ to a fiery end!$", RegexOptions.Compiled),
            new Regex(@"^You beat .+ to a lifeless pulp!$", RegexOptions.Compiled),
            new Regex(@"^You smite .+ mightily!$", RegexOptions.Compiled),
            new Regex(@"^You obliterate .+!$", RegexOptions.Compiled),
            new Regex(@"^You run .+ through!$", RegexOptions.Compiled),
            new Regex(@"^You reduce .+ to a sizzling, oozing mass!$", RegexOptions.Compiled),
            new Regex(@"^You knock .+ into next Morningthaw!$", RegexOptions.Compiled),
            new Regex(@"^You split .+ apart!$", RegexOptions.Compiled),
            new Regex(@"^You cleave .+ in twain!$", RegexOptions.Compiled),
            new Regex(@"^You slay .+ viciously enough to impart death several times over!$", RegexOptions.Compiled),
            new Regex(@"^You reduce .+ to a drained, twisted corpse!$", RegexOptions.Compiled),
            new Regex(@"^Your killing blow nearly turns .+ inside-out!$", RegexOptions.Compiled),
            new Regex(@"^Your attack stops .+ cold!$", RegexOptions.Compiled),
            new Regex(@"^Your lightning coruscates over .+'s mortal remains!$", RegexOptions.Compiled),
            new Regex(@"^Your assault sends .+ to an icy death!$", RegexOptions.Compiled),
            new Regex(@"^You killed .+!$", RegexOptions.Compiled),
            new Regex(@"^The thunder of crushing .+ is followed by the deafening silence of death!$", RegexOptions.Compiled),
            new Regex(@"^The deadly force of your attack is so strong that .+'s ancestors feel it!$", RegexOptions.Compiled),
            new Regex(@"^The force of your killing blow .+!$", RegexOptions.Compiled),
            new Regex(@"^.+?'s seared corpse smolders before you!$", RegexOptions.Compiled),
            new Regex(@"^.+ is reduced to cinders!$", RegexOptions.Compiled),
            new Regex(@"^.+ is shattered by your assault!$", RegexOptions.Compiled),
            new Regex(@"^.+ catches your attack, with dire consequences!$", RegexOptions.Compiled),
            new Regex(@"^.+ is utterly destroyed by your attack!$", RegexOptions.Compiled),
            new Regex(@"^.+ suffers a frozen fate!$", RegexOptions.Compiled),
            new Regex(@"^.+?'s perforated corpse falls before you!$", RegexOptions.Compiled),
            new Regex(@"^.+ is fatally punctured!$", RegexOptions.Compiled),
            new Regex(@"^.+?'s death is preceded by a sharp, stabbing pain!$", RegexOptions.Compiled),
            new Regex(@"^.+ is torn to ribbons by your assault!$", RegexOptions.Compiled),
            new Regex(@"^.+ is liquified by your attack!$", RegexOptions.Compiled),
            new Regex(@"^.+?'s last strength dissolves before you!$", RegexOptions.Compiled),
            new Regex(@"^Electricity tears .+ apart!$", RegexOptions.Compiled),
            new Regex(@"^Blistered by lightning, .+ falls!$", RegexOptions.Compiled),
            new Regex(@"^.+?'s last strength withers before you!$", RegexOptions.Compiled),
            new Regex(@"^.+ is dessicated by your attack!$", RegexOptions.Compiled),
            new Regex(@"^.+ is incinerated by your assault!$", RegexOptions.Compiled)
        };

        protected override void Startup()
        {
            try
            {
                CoreManager.Current.CharacterFilter.LoginComplete += CharacterFilter_LoginComplete;
                CoreManager.Current.ChatBoxMessage += Current_ChatBoxMessage;
                CoreManager.Current.RenderFrame += Current_RenderFrame;
                CoreManager.Current.CommandLineText += Current_CommandLineText;
            }
            catch (Exception ex)
            {
                Fail("Startup", ex);
            }
        }

        protected override void Shutdown()
        {
            try
            {
                if (CoreManager.Current != null)
                {
                    CoreManager.Current.RenderFrame -= Current_RenderFrame;
                    CoreManager.Current.ChatBoxMessage -= Current_ChatBoxMessage;
                    CoreManager.Current.CommandLineText -= Current_CommandLineText;

                    if (CoreManager.Current.CharacterFilter != null)
                        CoreManager.Current.CharacterFilter.LoginComplete -= CharacterFilter_LoginComplete;
                }
            }
            catch { }

            _loggedIn = false;
            _pendingKills.Clear();
            _recentXp.Clear();
        }

        private void CharacterFilter_LoginComplete(object sender, EventArgs e)
        {
            try
            {
                _lastTotalXp = CoreManager.Current.CharacterFilter.TotalXP;
                _pendingKills.Clear();
                _recentXp.Clear();
                _loggedIn = true;
                Chat("v" + Version + " loaded. XP-per-kill is ON.");
            }
            catch (Exception ex)
            {
                Fail("LoginComplete", ex);
            }
        }

        private void Current_ChatBoxMessage(object sender, ChatTextInterceptEventArgs e)
        {
            try
            {
                if (!_enabled || !_loggedIn || e == null || String.IsNullOrEmpty(e.Text))
                    return;

                string text = e.Text.Trim();

                if (!IsKillMessage(text))
                    return;

                DateTime now = DateTime.UtcNow;

                if (_debug)
                    Chat("DEBUG kill message: " + text);

                int xpIndex = FindRecentXpIndex(now);
                if (xpIndex >= 0)
                {
                    long delta = _recentXp[xpIndex].Delta;
                    _recentXp.RemoveAt(xpIndex);
                    OutputKillXp(delta);
                    return;
                }

                _pendingKills.Add(new KillMarker(now));
                CleanupOld(now);
            }
            catch (Exception ex)
            {
                Fail("ChatBoxMessage", ex);
            }
        }

        private void Current_RenderFrame(object sender, EventArgs e)
        {
            try
            {
                if (!_loggedIn)
                    return;

                long current = CoreManager.Current.CharacterFilter.TotalXP;

                if (current < _lastTotalXp)
                {
                    _lastTotalXp = current;
                    _pendingKills.Clear();
                    _recentXp.Clear();
                    return;
                }

                if (current > _lastTotalXp)
                {
                    long delta = current - _lastTotalXp;
                    _lastTotalXp = current;

                    DateTime now = DateTime.UtcNow;

                    if (_debug)
                        Chat("DEBUG XP change: +" + delta.ToString("N0", CultureInfo.InvariantCulture));

                    if (_enabled)
                    {
                        int killIndex = FindPendingKillIndex(now);
                        if (killIndex >= 0)
                        {
                            _pendingKills.RemoveAt(killIndex);
                            OutputKillXp(delta);
                        }
                        else
                        {
                            _recentXp.Add(new XpGain(now, delta));
                        }
                    }

                    CleanupOld(now);
                }
                else
                {
                    CleanupOld(DateTime.UtcNow);
                }
            }
            catch (Exception ex)
            {
                Fail("RenderFrame", ex);
            }
        }

        private static bool IsKillMessage(string text)
        {
            for (int i = 0; i < KillPatterns.Length; i++)
            {
                if (KillPatterns[i].IsMatch(text))
                    return true;
            }

            return false;
        }

        private int FindPendingKillIndex(DateTime now)
        {
            for (int i = 0; i < _pendingKills.Count; i++)
            {
                double age = (now - _pendingKills[i].Time).TotalMilliseconds;
                if (age >= 0 && age <= PairWindowMilliseconds)
                    return i;
            }

            return -1;
        }

        private int FindRecentXpIndex(DateTime now)
        {
            for (int i = _recentXp.Count - 1; i >= 0; i--)
            {
                double age = (now - _recentXp[i].Time).TotalMilliseconds;
                if (age >= 0 && age <= PairWindowMilliseconds)
                    return i;
            }

            return -1;
        }

        private void CleanupOld(DateTime now)
        {
            for (int i = _pendingKills.Count - 1; i >= 0; i--)
            {
                if ((now - _pendingKills[i].Time).TotalMilliseconds > PairWindowMilliseconds)
                {
                    if (_debug)
                        Chat("DEBUG kill marker expired without XP.");
                    _pendingKills.RemoveAt(i);
                }
            }

            for (int i = _recentXp.Count - 1; i >= 0; i--)
            {
                if ((now - _recentXp[i].Time).TotalMilliseconds > PairWindowMilliseconds)
                    _recentXp.RemoveAt(i);
            }
        }

        private void OutputKillXp(long delta)
        {
            if (delta <= 0)
                return;

            CoreManager.Current.Actions.AddChatText(
                "[Kill XP] " + delta.ToString("N0", CultureInfo.InvariantCulture) + " XP", 5);
        }

        private void Current_CommandLineText(object sender, ChatParserInterceptEventArgs e)
        {
            try
            {
                if (e == null || String.IsNullOrEmpty(e.Text))
                    return;

                string raw = e.Text.Trim();
                string cmd = raw.ToLowerInvariant();

                if (!cmd.StartsWith("/killxp"))
                    return;

                e.Eat = true;

                if (cmd == "/killxp on")
                {
                    _enabled = true;
                    Chat("ON");
                    return;
                }

                if (cmd == "/killxp off")
                {
                    _enabled = false;
                    _pendingKills.Clear();
                    _recentXp.Clear();
                    Chat("OFF");
                    return;
                }

                if (cmd == "/killxp status")
                {
                    Chat("enabled=" + _enabled +
                         ", debug=" + _debug +
                         ", pendingKills=" + _pendingKills.Count +
                         ", recentXp=" + _recentXp.Count);
                    return;
                }

                if (cmd == "/killxp debug on")
                {
                    _debug = true;
                    Chat("debug ON");
                    return;
                }

                if (cmd == "/killxp debug off")
                {
                    _debug = false;
                    Chat("debug OFF");
                    return;
                }

                if (cmd == "/killxp test")
                {
                    OutputKillXp(12345678);
                    return;
                }

                Chat("Commands: /killxp on | off | status | test | debug on | debug off");
            }
            catch (Exception ex)
            {
                Fail("Command", ex);
            }
        }

        private static void Chat(string text)
        {
            try
            {
                CoreManager.Current.Actions.AddChatText("[AC Kill XP] " + text, 5);
            }
            catch { }
        }

        private static void Fail(string where, Exception ex)
        {
            try
            {
                CoreManager.Current.Actions.AddChatText(
                    "[AC Kill XP] " + where + " error: " + ex.Message, 5);
            }
            catch { }
        }

        private sealed class KillMarker
        {
            public readonly DateTime Time;

            public KillMarker(DateTime time)
            {
                Time = time;
            }
        }

        private sealed class XpGain
        {
            public readonly DateTime Time;
            public readonly long Delta;

            public XpGain(DateTime time, long delta)
            {
                Time = time;
                Delta = delta;
            }
        }
    }
}
