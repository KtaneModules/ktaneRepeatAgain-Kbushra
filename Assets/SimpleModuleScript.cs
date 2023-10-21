using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KModkit;
using Newtonsoft.Json;
using System.Linq;
using System.Text.RegularExpressions;
using Rnd = UnityEngine.Random;

public class SimpleModuleScript : MonoBehaviour {

	public KMAudio audio;
	public KMBombInfo info;
	public KMBombModule module;
	public KMSelectable[] Button;
	static int ModuleIdCounter = 1;
	int ModuleId;

	public TextMesh[] screenTexts;

	private int SolveNum;
	private int SolveGoal;
	private string textFinder1;
	private string textFinder2;

	private int RandNum;
	private int RandGoal;

	private int strikecount;
	private int solvecount;

	private int TimeGoal = 0;
	private int TimeCheck;
	private int TimeEnd = -1;

	bool _isSolved = false;
	bool On = false;
	bool timeCreating = false;
	bool IsTimeModified = false;
	bool TimerOn = false;
	bool FailSafeOn = false;

	bool TimeModeActive;
	bool ZenModeActive;
	bool lightsOn;


	void Awake() 
	{
		ModuleId = ModuleIdCounter++;

		foreach (KMSelectable button in Button)
		{
			KMSelectable pressedButton = button;
			button.OnInteract += delegate () { buttonPress(pressedButton); return false; };
		}
		module.OnActivate = delegate() {lightsOn = true;};
	}

	void Start ()
	{
		strikecount = info.GetStrikes ();

		SolveGoal = Rnd.Range(3, 10);
		for (int i = 0; i < SolveGoal; i++)
			textFinder1 = SolveGoal.ToString();
		screenTexts[0].text = textFinder1;

		SolveNum = 0;
		for (int i = 0; i < SolveNum; i++)
			textFinder2 = SolveNum.ToString();
		screenTexts[1].text = textFinder2;

		RandGoal = Random.Range (1, 5001);
	}

	void FixedUpdate()
	{
		if(lightsOn == true)
		{
			if (On == false) 
			{
				TimerOn = false;
				screenTexts[2].text = "";

				timeCreating = false;
				RandNum = Random.Range (1, 5001);
				if (RandNum == RandGoal) 
				{
					audio.PlayGameSoundAtTransform (KMSoundOverride.SoundEffect.NeedyActivated, Button [0].transform);
					On = true;
				}
			}
			else 
			{
				screenTexts[2].text = "!";

				if (TimerOn == false) 
				{
					Invoke ("Timer", 0);
					TimerOn = true;
				}
			}

			for (int i = 0; i < SolveNum; i++)
				textFinder2 = SolveNum.ToString();
			screenTexts[1].text = textFinder2;

			if (SolveNum == SolveGoal) 
			{
				module.HandlePass ();
				_isSolved = true;
			}

			if (TimeEnd == (int) info.GetTime () && On == true) 
			{
				if (TimeModeActive == false) 
				{
					module.HandleStrike ();
					TimeEnd = -1;
					TimeGoal = 0;
					timeCreating = false;
					On = false;
				}
				else
				{
					if (strikecount != info.GetStrikes () || solvecount != info.GetSolvedModuleIDs().Count) 
					{
						Invoke ("Timer", 0);
						TimerOn = true;
						strikecount = info.GetStrikes ();
						solvecount = info.GetSolvedModuleIDs ().Count;
					}
					else
					{
						module.HandleStrike ();
						TimeEnd = -1;
						TimeGoal = 0;
						timeCreating = false;
						On = false;
					}
				}
			}

			if (((int)info.GetTime () <= 151 && FailSafeOn == false) && ZenModeActive == false) 
			{
				Invoke ("FailSafe", 1);
				FailSafeOn = true;
			}
			if (TimeModeActive == true) 
			{
				if (strikecount != info.GetStrikes () || solvecount != info.GetSolvedModuleIDs().Count) 
				{
					Invoke ("Timer", 0);
					TimerOn = true;
					strikecount = info.GetStrikes ();
					solvecount = info.GetSolvedModuleIDs ().Count;
				}
			}
		}
	}

	void FailSafe()
	{
		RandGoal = 0;
		TimeEnd = -1;
		TimeGoal = 0;
		On = false;
		timeCreating = false;
		TimerOn = false;

		Log ("Failsafe activated");

		On = true;
		SolveNum = SolveGoal - 1;
		audio.PlayGameSoundAtTransform (KMSoundOverride.SoundEffect.NeedyActivated, Button [0].transform);

		for (int i = 0; i < SolveGoal; i++)
			textFinder1 = SolveGoal.ToString();
		screenTexts[0].text = textFinder1;

		for (int i = 0; i < SolveNum; i++)
			textFinder2 = SolveNum.ToString();
		screenTexts[1].text = textFinder2;
	}

	void Timer()
	{
		if (ZenModeActive == true)
		{
			TimeEnd = (int) info.GetTime () + 150;
		}
		if (TimeModeActive == true) 
		{
			TimeEnd = (int) info.GetTime () - 90;
		}
		else
		{
			TimeEnd = (int) info.GetTime () - 150;
		}
	}

	void TimeCreator()
	{
		if (info.GetStrikes () == 0) 
		{
			TimeGoal = TimeGoal + 69;
			IsTimeModified = true;
		}
		if (info.GetStrikes () == 1) 
		{
			TimeGoal = TimeGoal + 21;
			IsTimeModified = true;
		}
		if (info.GetStrikes () == 2) 
		{
			TimeGoal = TimeGoal + 7;
			IsTimeModified = true;
		}
		if (info.GetStrikes () > 2) 
		{
			TimeGoal = TimeGoal + 666;
			IsTimeModified = true;
		}
		if (info.GetIndicators().ToList().Count > SolveNum) 
		{
			TimeGoal = TimeGoal + SolveGoal;
			IsTimeModified = true;
		}
		if (info.GetIndicators().ToList().Count < SolveNum) 
		{
			TimeGoal = TimeGoal - SolveGoal;
			IsTimeModified = true;
		}
		if (info.GetSolvedModuleIDs ().Contains ("BigButton"))
		{
			TimeGoal = TimeGoal * 2;
			IsTimeModified = true;
		}
		if (info.GetPortPlateCount() > SolveNum) 
		{
			TimeGoal = TimeGoal % 81;
			IsTimeModified = true;
		}
		if (info.GetSerialNumber().Contains("3")) 
		{
			TimeGoal = TimeGoal * 9;
			IsTimeModified = true;
		}
		if (IsTimeModified == false) 
		{
			TimeGoal = SolveNum;
		}
		TimeGoal = TimeGoal % 60;
		timeCreating = true;
	}

	void buttonPress(KMSelectable pressedButton)
	{
		int buttonPosition = new int();
		for(int i = 0; i < Button.Length; i++)
		{
			if (pressedButton == Button[i])
			{
				buttonPosition = i;
				break;
			}
		}

		if (On == true) 
		{
			audio.PlayGameSoundAtTransform (KMSoundOverride.SoundEffect.ButtonPress, Button [buttonPosition].transform);
			Button [buttonPosition].AddInteractionPunch ();
			switch (buttonPosition) 
			{
			case 0:
				if (_isSolved == false) 
				{
					if(timeCreating == false)
					{
						Invoke ("TimeCreator", 0);
					}

					if (timeCreating == true) 
					{
						if ((int) info.GetTime () % 60 == TimeGoal) 
						{
							SolveNum++;
							On = false;
						}
						else 
						{
							Debug.LogFormat ("Last two digits were {0} and time goal was {1}", (int) info.GetTime () % 60, TimeGoal);
							TimeGoal = 0;
							module.HandleStrike ();
							On = false;
							timeCreating = false;
							TimerOn = false;
						}
					}
				}
				break;
			}
		}
	}

	#pragma warning disable 414
	private readonly string TwitchHelpMessage = @"!{0} press (##) [Presses the button (optionally when the last two digits of the bomb's timer are '##')]";
	#pragma warning restore 414
	IEnumerator ProcessTwitchCommand(string command)
	{
		string[] parameters = command.Split(' ');
		if (Regex.IsMatch(parameters[0], @"^\s*press\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
		{
			if (parameters.Length > 2)
				yield return "sendtochaterror Too many parameters!";
			else if (parameters.Length == 2)
			{
				int temp = -1;
				if (!int.TryParse(parameters[1], out temp))
				{
					yield return "sendtochaterror!f The specified number '" + parameters[1] + "' is invalid!";
					yield break;
				}
				if (temp < 0 || temp > 59)
				{
					yield return "sendtochaterror The specified number '" + parameters[1] + "' is out of range 00-59!";
					yield break;
				}
				if (parameters[1].Length != 2)
				{
					yield return "sendtochaterror The specified number '" + parameters[1] + "' is not 2 digits long!";
					yield break;
				}
				if (!On)
				{
					yield return "sendtochaterror The button can only be pressed when the module is on!";
					yield break;
				}
				yield return null;
				while (temp != (int)info.GetTime() % 60) yield return "trycancel Halted waiting to press the button due to a cancel request!";
				Button[0].OnInteract();
			}
			else if (parameters.Length == 1)
			{
				if (!On)
				{
					yield return "sendtochaterror The button can only be pressed when the module is on!";
					yield break;
				}
				yield return null;
				Button[0].OnInteract();
			}
		}
	}

	void TwitchHandleForcedSolve()
	{
		StartCoroutine(HandleAutoSolve());
	}

	IEnumerator HandleAutoSolve()
	{
		while (!_isSolved)
		{
			while (!On) yield return null;
			pressAgain:
			Button[0].OnInteract();
			yield return null;
			while ((int)info.GetTime() % 60 != TimeGoal)
			{
				yield return null;
				if (!timeCreating)
					goto pressAgain;
			}
			Button[0].OnInteract();
			yield return null;
		}
	}

	void Log(string message)
	{
		Debug.LogFormat("[Again #{0}] {1}", ModuleId, message);
	}

	//twitch plays
	#pragma warning disable 414
	private readonly string TwitchHelpMessage = @"!{0} press (##) [Presses the button (optionally when the last two digits of the bomb's timer are '##')]";
	#pragma warning restore 414
	IEnumerator ProcessTwitchCommand(string command)
	{
		string[] parameters = command.Split(' ');
		if (Regex.IsMatch(parameters[0], @"^\s*press\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
		{
			if (parameters.Length > 2)
				yield return "sendtochaterror Too many parameters!";
			else if (parameters.Length == 2)
			{
				int temp = -1;
				if (!int.TryParse(parameters[1], out temp))
				{
					yield return "sendtochaterror!f The specified number '" + parameters[1] + "' is invalid!";
					yield break;
				}
				if (temp < 0 || temp > 59)
				{
					yield return "sendtochaterror The specified number '" + parameters[1] + "' is out of range 00-59!";
					yield break;
				}
				if (parameters[1].Length != 2)
				{
					yield return "sendtochaterror The specified number '" + parameters[1] + "' is not 2 digits long!";
					yield break;
				}
				if (!On)
                {
					yield return "sendtochaterror The button can only be pressed when the module is on!";
					yield break;
				}
				yield return null;
				while (temp != (int)info.GetTime() % 60) yield return "trycancel Halted waiting to press the button due to a cancel request!";
				Button[0].OnInteract();
			}
			else if (parameters.Length == 1)
			{
				if (!On)
				{
					yield return "sendtochaterror The button can only be pressed when the module is on!";
					yield break;
				}
				yield return null;
				Button[0].OnInteract();
			}
		}
	}

	void TwitchHandleForcedSolve()
	{
		StartCoroutine(HandleAutoSolve());
	}

	IEnumerator HandleAutoSolve()
    {
		while (!_isSolved)
        {
			while (!On) yield return null;
			pressAgain:
			Button[0].OnInteract();
			yield return null;
			while ((int)info.GetTime() % 60 != TimeGoal)
            {
				yield return null;
				if (!timeCreating)
					goto pressAgain;
            }
			Button[0].OnInteract();
			yield return null;
		}
    }
}

