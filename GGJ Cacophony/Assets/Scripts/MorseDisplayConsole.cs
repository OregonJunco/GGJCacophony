﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

[RequireComponent(typeof(Text))]
public class MorseDisplayConsole : MonoBehaviour
{
    private Text mText;
    private const int displayRows = 6;
    private const int readingHeadOffset = 10;
    private const string borderDeco = "# ";
    private const string processingTest = "PROCESSING SIGNAL AT 1.0X SPEED";
    private const string readingCurrentTest = "YOU ARE READING THE CURRENT MESSAGE";
    private const string pressTabMessageTest = "PRESS <TAB> TO ACCEPT NEW MESSAGE";

    private void Start ()
    {
        mText = GetComponent<Text>();
        drawBorders();
	}

    private void Update()
    {
        drawBorders();
    }

    private void drawBorders ()
    {
        int textAreaWidth = TextLog.instance.GetLineWidthOfTextField();
        int noDecoWidth = getNoDecoWidth(textAreaWidth);
        int playbackIndex = 0;

        mText.text = "";
        
        for(int r = 0; r < displayRows; r++) {
            mText.text += borderDeco;
            if (r == 0 || r == displayRows - 1) {
                for (int i = 0; i < noDecoWidth; i++) {
                    mText.text += '=';
                }
            }
            else if(r == 1) {
                mText.text += drawStringWithPadding(Mathf.FloorToInt((float)noDecoWidth / 2f), processingTest);
                mText.text += drawStringWithPadding(Mathf.CeilToInt ((float)noDecoWidth / 2f), readingCurrentTest);
            }
            else if (r == 2) {
                mText.text += drawStringWithPadding(Mathf.FloorToInt((float)noDecoWidth / 2f), "");
                mText.text += drawStringWithPadding(Mathf.CeilToInt((float)noDecoWidth / 2f), pressTabMessageTest);
            }
            else if (r == 3) {
                for(int i = 0; i < readingHeadOffset - 1; i++) {
                    mText.text += ' ';
                }
                mText.text += 'V';
                for (int i = 0; i < noDecoWidth - readingHeadOffset; i++) {
                    mText.text += ' ';
                }
            }
            else if (r == 4) {
                string playbackStream = ""; 
                for (int i = 0; i < readingHeadOffset; i++) {
                    playbackStream += ' ';
                }

                playbackStream += MorseAudioController.instance.GetPlaybackStateString(noDecoWidth * 2f, out playbackIndex);
                Debug.Log("playbackIndex is " + playbackIndex + " and PlaybackStream is " + playbackStream);

                if(playbackStream.Length > readingHeadOffset) {
                    playbackStream = playbackStream.Substring(playbackIndex, noDecoWidth);
                    mText.text += playbackStream;
                }
                else {
                    for(int i = 0; i < noDecoWidth; i++) {
                        mText.text += ' ';
                    }
                }
            }

            mText.text += borderDecoReverse();
            mText.text += '\n';
        }
	}

    private string drawStringWithPadding(int area, string str)
    {
        if(str.Length > area) {
            Debug.LogError("Can't pad a string with more width than the target area!");
            return str;
        }

        string output = "";

        float padding = getElementPadding(area, str);
        for (int i = 0; i < Mathf.Floor(padding); i++) {
            output += ' ';
        }
        output += str;
        for (int i = 0; i < Mathf.Ceil(padding); i++) {
            output += ' ';
        }

        return output;
    }

    private int getNoDecoWidth(int textAreaWidth)
    {
        return textAreaWidth - 2 * (borderDeco.Length);
    }

    private float getElementPadding(int areaWidth, string element)
    {
        return (float)(areaWidth - element.Length) / 2f;
    }

    private string borderDecoReverse()
    {
        char[] charArray = borderDeco.ToCharArray();
        System.Array.Reverse(charArray);
        return new string(charArray);
    }
}