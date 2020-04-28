using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace SmartBulbColor.RemoteBulbAPI
{
    public static class BulbCommandBuilder
    {
        public static BulbCommand CreateGetPropertiesCommand(params BulbProperties[] properties)
        {
            if (properties.Length < 1)
                throw new Exception("Command Properties can't be empty!");

            ArrayList array = new ArrayList();
            foreach (var property in properties)
            {
                array.Add(property.ToString());
            }
            return new BulbCommand("get_prop", array, ResponseMode.FullResponse);
        }
        public static BulbCommand CreateSetColorTemperatureCommand(int colorTemperature, Effect effect, int duration)
        {
            ArrayList array = new ArrayList();
            array.Add(colorTemperature);
            array.Add(effect.ToString());
            array.Add(duration);
            return new BulbCommand("set_ct_abx", array, ResponseMode.None);
        }
        public static BulbCommand CreateSetRgbCommand(int rgbDecimal, Effect effect, int duration)
        {
            ArrayList array = new ArrayList();
            array.Add(rgbDecimal);
            array.Add(effect.ToString());
            array.Add(duration);
            return new BulbCommand("set_rgb", array, ResponseMode.None);
        }
        public static BulbCommand CreateSetHsvCommand(int hue, int saturation, Effect effect, int duration)
        {
            ArrayList array = new ArrayList();
            array.Add(hue);
            array.Add(saturation);
            array.Add(effect.ToString());
            array.Add(duration);
            return new BulbCommand("set_hsv", array, ResponseMode.None);
        }
        public static BulbCommand CreateSetBrightnessCommand(int brightness, Effect effect, int duration)
        {
            ArrayList array = new ArrayList();
            array.Add(brightness);
            array.Add(effect.ToString());
            array.Add(duration);
            return new BulbCommand("set_bright", array, ResponseMode.None);
        }
        public static BulbCommand CreateSetPowerCommand(Power power, Effect effect, int duration, ColorMode mode)
        {
            ArrayList array = new ArrayList();
            array.Add(power.ToString());
            array.Add(effect.ToString());
            array.Add(duration);
            array.Add((int)mode);
            return new BulbCommand("set_power", array, ResponseMode.IsOk);
        }
        public static BulbCommand CreateToggleCommand() //TODO: check if empty an array works for json []
        {
            ArrayList array = new ArrayList();
            return new BulbCommand("toggle", array, ResponseMode.IsOk);
        }
        public static BulbCommand CreateStartColorFlowCommand(int count, FlowEndsWith action, params ColorFlowTurple[] colorFlows)
        {
            ArrayList array = new ArrayList();
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < colorFlows.Length; i++)
            {
                builder.Append(colorFlows[i].Duration.ToString());
                builder.Append(", ");
                builder.Append(colorFlows[i].Mode.ToString());
                builder.Append(", ");
                builder.Append(colorFlows[i].Value.ToString());
                builder.Append(", ");
                builder.Append(colorFlows[i].Brightness.ToString());
                if (i != colorFlows.Length - 1)
                    builder.Append(", ");
            }
            array.Add(count);
            array.Add((int)action);
            array.Add(builder.ToString());
            return new BulbCommand("start_cf", array, ResponseMode.IsOk);
        }
        public static BulbCommand CreateStopColorFlowCommand() //TODO: check if empty an array works for json []
        {
            ArrayList array = new ArrayList();
            return new BulbCommand("stop_cf", array, ResponseMode.IsOk);
        }
        public static BulbCommand CreateSetSceneColorCommand(int decimalRgb, int brightness)
        {
            ArrayList array = new ArrayList();
            array.Add("color");
            array.Add(decimalRgb);
            array.Add(brightness);
            return new BulbCommand("set_scene", array, ResponseMode.None);
        }
        public static BulbCommand CreateSetSceneColorTemperatureCommand(int colorTemperature, int brightness)
        {
            ArrayList array = new ArrayList();
            array.Add("ct");
            array.Add(colorTemperature);
            array.Add(brightness);
            return new BulbCommand("set_scene", array, ResponseMode.None);
        }
        public static BulbCommand CreateSetSceneHsvCommand(int hue, int saturation, int brightness)
        {
            ArrayList array = new ArrayList();
            array.Add("hsv");
            array.Add(hue);
            array.Add(saturation);
            array.Add(brightness);
            return new BulbCommand("set_scene", array, ResponseMode.None);
        }
        public static BulbCommand CreateSetSceneColorFlowCommand(int count, FlowEndsWith action, params ColorFlowTurple[] colorFlows)
        {
            ArrayList array = new ArrayList();
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < colorFlows.Length; i++)
            {
                builder.Append(colorFlows[i].Duration.ToString());
                builder.Append(", ");
                builder.Append(colorFlows[i].Mode.ToString());
                builder.Append(", ");
                builder.Append(colorFlows[i].Value.ToString());
                builder.Append(", ");
                builder.Append(colorFlows[i].Brightness.ToString());
                if (i != colorFlows.Length - 1)
                    builder.Append(", ");
            }
            array.Add("cf");
            array.Add(count);
            array.Add((int)action);
            array.Add(builder.ToString());
            return new BulbCommand("set_scene", array, ResponseMode.IsOk);
        }
        public static BulbCommand CreateSetSceneAutoDelayOffCommand(int brightness, int delayOff)
        {
            ArrayList array = new ArrayList();
            array.Add("auto_delay_off");
            array.Add(brightness);
            array.Add(delayOff);
            return new BulbCommand("set_scene", array, ResponseMode.IsOk);
        }
        public static BulbCommand CreateAddSleepTimerCommand(TimerType type, int time)
        {
            ArrayList array = new ArrayList();
            array.Add((int)type);
            array.Add(time);
            return new BulbCommand("cron_add", array, ResponseMode.IsOk);
        }
        public static BulbCommand CreateGetSleepTimerCommand(TimerType type)
        {
            ArrayList array = new ArrayList();
            array.Add((int)type);
            return new BulbCommand("cron_get", array, ResponseMode.FullResponse);
        }
        public static BulbCommand CreateDeleteSleepTimerCommand(TimerType type)
        {
            ArrayList array = new ArrayList();
            array.Add((int)type);
            return new BulbCommand("cron_del", array, ResponseMode.IsOk);
        }
        /// <summary>
        /// Sets particular property Adjust. 
        /// NOTE: When “prop" is “color", the “action" can only be “circle", otherwise, it will be deemed as invalid request!
        /// </summary>
        /// <param name="action"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        public static BulbCommand CreateSetAdjustCommand(AdjustAction action, AdjustProperty property)
        {
            if (property == AdjustProperty.color && action != AdjustAction.circle)
                throw new Exception("The 'color' property supports no settings but 'circle'!");
            ArrayList array = new ArrayList();
            array.Add(action.ToString());
            array.Add(property.ToString());
            return new BulbCommand("set_adjust", array, ResponseMode.None);
        }
        public static BulbCommand CreateSetMusicModeCommand(MusicModeAction action, string ip, int port)
        {
            ArrayList array = new ArrayList();
            array.Add((int)action);
            array.Add(ip);
            array.Add(port);
            return new BulbCommand("set_music", array, ResponseMode.IsOk);
        }
    }
}
