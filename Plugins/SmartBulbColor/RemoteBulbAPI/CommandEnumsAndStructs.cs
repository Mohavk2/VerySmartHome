using System;
using System.Collections.Generic;
using System.Text;

namespace SmartBulbColor.RemoteBulbAPI
{
	public enum Power { On, Off }
	public enum Effect { Sudden, Smooth }
	public enum BulbProperties
	{
		power,          //on: smart LED is turned on / off: smart LED is turned off
		bright,         //Brightness percentage. Range 1 ~ 100
		ct,             //Color temperature. Range 1700 ~ 6500(k)
		rgb,            //Color. Range 1 ~ 16777215
		hue,            //Hue. Range 0 ~ 359
		sat,            //Saturation. Range 0 ~ 100
		color_mode,     //1: rgb mode / 2: color temperature mode / 3: hsv mode
		flowing,        //0: no flow is running / 1:color flow is running
		delayoff,       //The remaining time of a sleep timer. Range 1 ~ 60 (minutes)
		flow_params,    //Current flow parameters (only meaningful when 'flowing' is 1)
		music_on,       //1: Music mode is on / 0: Music mode is off
		name,           //The name of the device set by “set_name” command
	}
	public enum ColorMode
	{
		Default = 0,
		CT = 1,
		RGB = 2,
		HSV = 3,
		FLOW = 4
	}
	public enum ColorFlowMode
	{
		Color = 1,
		ColorTemperature = 2,
		Sleep = 7
	}
	/// <summary>
	/// The Value can be decimal rgb color or color temperature value. It depends on chosen mode.
	/// </summary>
	public struct ColorFlowTurple
	{
		public int Duration { get; }
		public int Mode { get; }
		public int Value { get; }
		public int Brightness { get; }

		public ColorFlowTurple(int duration, ColorFlowMode mode, int value, int brightness)
		{
			Duration = duration;
			Mode = (int)mode;
			Value = value;
			Brightness = brightness;
		}
	}
	public enum FlowEndsWith
	{
		RecoverState = 0,
		Continue = 1,
		TurnOff = 2,
	}
	public enum TimerType
	{
		Off = 0
	}
	public enum AdjustAction
	{
		increase,
		decrease,
		circle
	}
	public enum AdjustProperty
	{
		bright,
		ct,
		color
	}
	public enum MusicModeAction
	{
		Off = 0,
		On = 1
	}
	public enum ResponseMode
	{
		None,
		IsOk,
		FullResponse
	}
}
