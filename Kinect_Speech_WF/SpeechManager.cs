// <copyright file="SpeechManager.cs" company="dyadica.co.uk">
// Copyright (c) 2010, 2014 All Right Reserved, http://www.dyadica.co.uk

// This source is subject to the dyadica.co.uk Permissive License.
// Please see the http://www.dyadica.co.uk/permissive-license file for more information.
// All other rights reserved.

// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.
//
// </copyright>

// <author>SJB</author>
// <email>contact via facebook.com/adropinthedigitalocean</email>
// <date>31.05.2014</date>
// <summary>A simple c# class containing code required to perform speech recognition
// with the Kinect SDK Version 1.8
// Based on and using code from http://msdn.microsoft.com/en-us/library/hh855387.aspx
// </summary

using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Kinect;
using Microsoft.Kinect.Interop;

using Microsoft.Speech;
using Microsoft.Speech.AudioFormat;
using Microsoft.Speech.Recognition;

using System.IO;

namespace Kinect_Speech_WF
{
    public class SpeechManager
    {
        /// <summary>
        /// Active Kinect sensor.
        /// </summary>
        public KinectSensor Sensor;

        /// <summary>
        /// Speech recognition engine using audio data from Kinect.
        /// </summary>
        public SpeechRecognitionEngine SpeechEngine;

        /// <summary>
        /// The path location for the required grammar file.
        /// </summary>
        public string GrammarFile;

        /// <summary>
        /// Basic constructor that accepts both a reference to the
        /// Kinect sensor and the location of the grammar file to
        /// be used by the speech engine.
        /// </summary>
        /// <param name="sensor"></param>
        /// <param name="grammarFile"></param>
        public SpeechManager(KinectSensor sensor, string grammarFile)
        {
            // Set the sensor to that passed in.
            Sensor = sensor;
            // Set the location of the grammar
            // to be used.
            GrammarFile = grammarFile;
            // Set things going.
            StartRecognisingSpeech();
        }

        /// <summary>
        /// Basic constructor that accepts a reference to the
        /// Kinect sensor and then generates a default path
        /// to the location of the grammar file to be used by 
        /// the speech engine.
        /// </summary>
        /// <param name="sensor"></param>
        public SpeechManager(KinectSensor sensor)
        {
            // Set the sensor to that passed in.
            Sensor = sensor;
            // Set the location of the grammar to
            // a known default in the same location
            // as the application.
            GrammarFile = AppDomain.CurrentDomain.BaseDirectory + "Grammar.xml";
            // Set things going.
            StartRecognisingSpeech();
        }

        /// <summary>
        /// Initialise the speech recognition and set everything
        /// going.
        /// </summary>
        public void StartRecognisingSpeech()
        {
            // If we are creating a win form app then this code could
            // be used to access a static instance of the kinect sensor
            // however for now I just pass it in etc.

            // if (Form1.Sensor != null) { this.sensor = Form1.Sensor; }

            RecognizerInfo recognizerInfo = GetKinectRecognizer();

            if (recognizerInfo != null)
            {
                SpeechEngine = new SpeechRecognitionEngine(recognizerInfo.Id);

                string path = AppDomain.CurrentDomain.BaseDirectory;

                GrammarBuilder builder = 
                    new GrammarBuilder { Culture = recognizerInfo.Culture };

                // The following code shows how to build a grammar file
                // programaticaly. However its really easy to just load
                // in a grammar file created in xml by supplying the
                // path to the file. In reality this makes much more sense!

                // Choices choices = new Choices();

                // choices.Add(new SemanticResultValue("forward", "FORWARD"));
                // choices.Add(new SemanticResultValue("forwards", "FORWARD"));
                // choices.Add(new SemanticResultValue("ahead", "FORWARD"));

                // choices.Add(new SemanticResultValue("backward", "BACKWARD"));
                // choices.Add(new SemanticResultValue("backwards", "BACKWARD"));
                // choices.Add(new SemanticResultValue("back", "BACKWARD"));

                // choices.Add(new SemanticResultValue("turn left", "LEFT"));
                // choices.Add(new SemanticResultValue("left", "LEFT"));

                // choices.Add(new SemanticResultValue("turn right", "RIGHT"));
                // choices.Add(new SemanticResultValue("right", "RIGHT"));

                // builder.Append(choices);

                // Grammar grammar = new Grammar(builder);

                // Create a grammar from grammar definition XML file.

                Grammar grammar = new Grammar(path + @"\Grammar.xml");

                SpeechEngine.LoadGrammar(grammar);

                // Register for events

                SpeechEngine.SpeechRecognized +=
                    SpeechEngine_SpeechRecognized;

                SpeechEngine.SpeechRecognitionRejected +=
                    SpeechEngine_SpeechRejected;

                SpeechEngine.SpeechHypothesized +=
                    SpeechEngine_SpeechHypothesized;

                SpeechEngine.LoadGrammarCompleted +=
                    SpeechEngine_LoadGrammarCompleted;

                // For long recognition sessions (a few hours or more), 
                // it may be beneficial to turn off adaptation of the 
                // acoustic model. This will prevent recognition accuracy 
                // from degrading over time.

                // speechEngine.UpdateRecognizerSetting("AdaptationOn", 0);

                SpeechEngine.SetInputToAudioStream(Sensor.AudioSource.Start(),
                    new SpeechAudioFormatInfo(EncodingFormat.Pcm, 16000, 16, 1, 32000, 2, null));

                // Specifies that recognition does not terminate after 
                // completion.

                SpeechEngine.RecognizeAsync(RecognizeMode.Multiple);                        
            }
            else
            {
                // Failed to find a recogniser so provide
                // some detail here!

                System.Diagnostics.Debug.WriteLine("No Speech Recogniser Found!");
            }
        }

        /// <summary>
        /// Function used to stop the speech recogniser
        /// and clean up afterwards.
        /// </summary>
        public void StopRecognisingSpeech()
        {
            // Remove the events

            SpeechEngine.SpeechRecognized -=
                    SpeechEngine_SpeechRecognized;

            SpeechEngine.SpeechRecognitionRejected -=
                SpeechEngine_SpeechRejected;

            SpeechEngine.SpeechHypothesized -=
                SpeechEngine_SpeechHypothesized;

            SpeechEngine.LoadGrammarCompleted -=
                SpeechEngine_LoadGrammarCompleted;

            // Stop the recognition

            SpeechEngine.RecognizeAsyncStop();
            // SpeechEngine.Dispose();
        }

        /// <summary>
        /// Gets the metadata for the speech recognizer (acoustic model) most suitable to
        /// process audio from Kinect device.
        /// </summary>
        /// <returns>
        /// RecognizerInfo if found, <code>null</code> otherwise.
        /// </returns>
        private static RecognizerInfo GetKinectRecognizer()
        {
            foreach (RecognizerInfo recognizer in SpeechRecognitionEngine.InstalledRecognizers())
            {
                string value;
                recognizer.AdditionalInfo.TryGetValue("Kinect", out value);
                if ("True".Equals(value, StringComparison.OrdinalIgnoreCase) && "en-US".Equals(recognizer.Culture.Name, StringComparison.OrdinalIgnoreCase))
                {
                    return recognizer;
                }
            }

            return null;
        }

        /// <summary>
        /// Handler for loaded grammar speech event.
        /// </summary>
        /// <param name="sender">object sending the event.</param>
        /// <param name="e">event arguments.</param>
        private void SpeechEngine_LoadGrammarCompleted(object sender, LoadGrammarCompletedEventArgs e)
        {
            // throw new NotImplementedException();
            System.Diagnostics.Debug.WriteLine("Grammar loaded: " + e.Grammar.Name);
        }

        /// <summary>
        /// Handler for recognized speech events.
        /// </summary>
        /// <param name="sender">object sending the event.</param>
        /// <param name="e">event arguments.</param>
        private void SpeechEngine_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            // Speech utterance confidence below which we 
            // treat speech as if it hadn't been heard.

            const double ConfidenceThreshold = 0.3;

            if (e.Result.Confidence >= ConfidenceThreshold)
            {
                // System.Diagnostics.Debug.WriteLine(e.Result.Text);
            }
        }

        /// <summary>
        /// Handler for rejected speech events.
        /// </summary>
        /// <param name="sender">object sending the event.</param>
        /// <param name="e">event arguments.</param>
        private void SpeechEngine_SpeechRejected(object sender, SpeechRecognitionRejectedEventArgs e)
        {
            // System.Diagnostics.Debug.WriteLine("?");
        }

        /// <summary>
        /// Handler for hypothesized speech events.
        /// </summary>
        /// <param name="sender">object sending the event.</param>
        /// <param name="e">event arguments.</param>
        private void SpeechEngine_SpeechHypothesized(object sender, SpeechHypothesizedEventArgs e)
        {
            // System.Diagnostics.Debug.WriteLine(e.Result.Text);
        }
    }
}
