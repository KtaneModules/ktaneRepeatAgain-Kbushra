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

	public int RandNum;
	public int RandGoal;

	private int strikecount;
	private int solvecount;

	public int TimeGoal = 0;
	public int TimeCheck;
	public int TimeEnd;

	bool _isSolved = false;
	bool On = false;
	bool timeCreating = false;
	bool IsTimeModified = false;
	bool TimerOn = false;
	bool FailSafeOn = false;

	bool TimeModeActive;
	bool ZenModeActive;


	void Awake() 
	{
		ModuleId = ModuleIdCounter++;

		foreach (KMSelectable button in Button)
		{
			KMSelectable pressedButton = button;
			button.OnInteract += delegate () { buttonPress(pressedButton); return false; };
		}
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

		RandGoal = Random.Range (1, 2001);
	}

	void FixedUpdate()
	{
		if (On == false) 
		{
			TimerOn = false;
			screenTexts[2].text = "";

			timeCreating = false;
			RandNum = Random.Range (1, 2001);
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

		if (TimeEnd == (int) info.GetTime ()) 
		{
			if (TimeModeActive == false) 
			{
				module.HandleStrike ();
				TimeEnd = 0;
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
					TimeEnd = 0;
					TimeGoal = 0;
					timeCreating = false;
					On = false;
				}
			}
		}

		if ((int)info.GetTime () <= 151 && FailSafeOn == false) 
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

	void FailSafe()
	{
		RandGoal = 0;
		TimeEnd = 0;
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
		TimeEnd = 0;
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

	void Log(string message)
	{
		Debug.LogFormat("[Again #{0}] {1}", ModuleId, message);
	}
}

