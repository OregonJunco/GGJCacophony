﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MorsePlaybackState { stopped, playingDot, playingDash, elementBreak, letterBreak, wordBreak, messageBreak}

[RequireComponent(typeof(AudioSource))]
public class MorseAudioController : MonoBehaviour
{
    [SerializeField] private float dotLength = 0.2f;
    [SerializeField] private float minSpeed = 0.3f;
    [SerializeField] private float maxSpeed = 2f;


    public static MorseAudioController _instance;
    public static MorseAudioController instance {
        get {
            if (_instance == null) {
                _instance = GameObject.FindObjectOfType<MorseAudioController>();
            }
            return _instance;
        }
    }

    public  AudioSource mAudioSource;
    private string currentSentence;

    private string[][] currentMorseMessage;
    private Queue<string[][]> upcomingMorseMessages = new Queue<string[][]>();

    public int wordIndex { get; private set; }
    public int letterIndex { get; private set; }
    public int charIndex { get; private set; }

    public static bool morsePlaying;

    public bool playingOrPaused { get; private set;}
    private float timer = 0f;
    private bool needElementSeparator = false;
    private bool playingCharacter = false;
    private bool betweenMessages = false;
    private MorsePlaybackState playbackState = MorsePlaybackState.stopped;
    public float morsePlaybackScalar { get; private set; }

    void Start()
    {
        mAudioSource = GetComponent<AudioSource>();
        upcomingMorseMessages = new Queue<string[][]>();
        morsePlaybackScalar = 1f;
        playingOrPaused = true;
    }

    public void EnqueueMorseString(string morseStringIn)
    {
        string[][] morseLetters = MorseUtility.GetMorseWordLetters(MorseUtility.GenerateMorseSentence(morseStringIn));
        string morseSentence = MorseUtility.GenerateMorseSentence(morseStringIn);
        if (!string.IsNullOrEmpty(morseSentence)){
            upcomingMorseMessages.Enqueue(morseLetters);
        }
        
    }

    private void playNextMorseString()
    {
        if (upcomingMorseMessages.Count >= 1) {
            currentMorseMessage = upcomingMorseMessages.Dequeue();
            resetMorseSequence();
        }
    }

    public void resetMorseSequence()
    {
        timer = 0f;
        wordIndex = 0;
        letterIndex = 0;
        charIndex = 0;
        needElementSeparator = false;
        playingCharacter = false;
        betweenMessages = false;
    }

    private void Update()
    {
        interpretControlInput();
        manageMorseStateMachine();
    }

    private void interpretControlInput()
    {
        if (Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.Underscore) || Input.GetKeyDown(KeyCode.KeypadMinus)) {
            IncrementTimescale(false);
        }
        if (Input.GetKeyDown(KeyCode.Plus) || Input.GetKeyDown(KeyCode.Equals) || Input.GetKeyDown(KeyCode.KeypadPlus)) {
            IncrementTimescale(true);
        }
        if (Input.GetKeyDown(KeyCode.LeftBracket)) {
            MoveReadingHead(false);
        }
        if (Input.GetKeyDown(KeyCode.RightBracket)) {
            MoveReadingHead(true);
        }
        if (Input.GetKeyDown(KeyCode.Backslash)) {
            if(currentMorseMessage != null) {
                playingOrPaused = !playingOrPaused;
            }
        }
        if (Input.GetKeyDown(KeyCode.Tab)) {
            playNextMorseString();
        }
    }

    private void manageMorseStateMachine()
    {
        //I hate this:
        if (currentMorseMessage != null && playingOrPaused) {
            timer -= Time.deltaTime * morsePlaybackScalar;
            if (timer <= 0) {
                betweenMessages = false;
                if (needElementSeparator) {
                    //Debug.Log("Playing element break at Time " + Time.time);
                    timer = dotLength * 1f;
                    mAudioSource.Stop();
                    morsePlaying = false;
                    needElementSeparator = false;
                }
                else {
                    if (playingCharacter) {
                        charIndex++;
                    }
                    if (charIndex < currentMorseMessage[wordIndex][letterIndex].Length) {
                        switch (currentMorseMessage[wordIndex][letterIndex][charIndex]) {
                            case '.':
                                //Play Dot:
                                //Debug.Log("Playing dot at Time " + Time.time);
                                timer = dotLength * 1f;
                                mAudioSource.Play();
                                morsePlaying = true;
                                playingCharacter = true;
                                if (charIndex < currentMorseMessage[wordIndex][letterIndex].Length - 1) {
                                    needElementSeparator = true;
                                }
                                break;
                            case '-':
                                //Play Dash:
                                //Debug.Log("Playing Dash at Time " + Time.time);
                                timer = dotLength * 3f;
                                mAudioSource.Play();
                                morsePlaying = true;
                                playingCharacter = true;
                                if (charIndex < currentMorseMessage[wordIndex][letterIndex].Length - 1) {
                                    needElementSeparator = true;
                                }
                                break;
                        }
                    }
                    else {
                        mAudioSource.Stop();
                        morsePlaying = false;
                        charIndex = 0;
                        letterIndex++;
                        timer = dotLength * MorseUtility.spaceBetweenLetters;
                        if (letterIndex >= currentMorseMessage[wordIndex].Length) {
                            letterIndex = 0;
                            wordIndex++;
                            timer = dotLength * MorseUtility.spaceBetweenWords;
                            if (wordIndex >= currentMorseMessage.Length) {
                                wordIndex = 0;
                                timer = dotLength * MorseUtility.spaceBetweenMessages;
                                betweenMessages = true;
                                playingCharacter = false;
                                //Debug.Log("Playing message break at Time " + Time.time);
                            }
                            else {
                                playingCharacter = false;
                                //Debug.Log("Playing word break at Time " + Time.time);
                            }
                        }
                        else {
                            playingCharacter = false;
                            //Debug.Log("Playing Letter break at Time " + Time.time);
                        }
                    }

                    Debug.Log("The current letter is \"" + currentMorseMessage[wordIndex][letterIndex] + "\"");
                }
            }
        }
    }

    private void MoveReadingHead(bool forward)
    {
        if(currentMorseMessage != null) {
            //Small delay:
            mAudioSource.Stop();
            morsePlaying = false;
            timer = dotLength * 6f;

            needElementSeparator = false;
            playingCharacter = false;
            betweenMessages = false;

            if (forward) {
                letterIndex++;
                charIndex = 0;
                if (letterIndex >= currentMorseMessage[wordIndex].Length) {
                    letterIndex = 0;
                    wordIndex++;
                    if(wordIndex >= currentMorseMessage.Length) {
                        wordIndex = 0;
                    }
                }
            }
            else {
                if(charIndex > 0) {
                    charIndex = 0;
                }
                else {
                    letterIndex--;
                    if(letterIndex < 0) {
                        wordIndex--;
                        if(wordIndex < 0) {
                            wordIndex = currentMorseMessage.Length - 1;
                        }
                        letterIndex = currentMorseMessage[wordIndex].Length - 1;
                    }
                }
            }
        }
    }

    public void IncrementTimescale(bool add)
    {
        morsePlaybackScalar += add ? 0.1f : -0.1f;
        morsePlaybackScalar = Mathf.Clamp(morsePlaybackScalar, minSpeed, maxSpeed);
    }

    public string GetPlaybackStateString(float minWidth, out int currentLetterIndex)
    {
        currentLetterIndex = 0;
        if(currentMorseMessage == null) {
            currentLetterIndex = 0;
            return "";
        }

        string output = "";

        int elementCounter = 0;

        for(int i = 0; i < currentMorseMessage.Length; i++) {
            for(int j = 0; j < currentMorseMessage[i].Length; j++) {
                output += 'X';
                elementCounter++;
                if(i == wordIndex && j == letterIndex) {
                    currentLetterIndex = elementCounter;
                }
            }

            if(i < currentMorseMessage.Length - 1) {
                output += ' ';
                elementCounter++;
            }
            else {
                for(int j = 0; j < MorseUtility.spaceBetweenMessages; j++)
                output += " ";
            }
        }
        
        while(output.Length < minWidth && output.Length > 0) {
            output += output;
        }

        if (betweenMessages) {
            int totalElements = 0;
            for(int j = 0; j < currentMorseMessage.Length; j++) {
                for(int k = 0; k < currentMorseMessage[j].Length; k++) {
                    totalElements++;
                }
            }

            currentLetterIndex = 1 + totalElements + (int)(MorseUtility.spaceBetweenMessages  * (1f - timer / (dotLength * MorseUtility.spaceBetweenMessages)));
        }
        return output;
    }

    public bool HasMessage()
    {
        return currentMorseMessage == null;
    }

    public int GetQueuedMessageCount()
    {
        return upcomingMorseMessages.Count;
    }

    public bool IsBetweenMessages()
    {
        return betweenMessages;
    }
}