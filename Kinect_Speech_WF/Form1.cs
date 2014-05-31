// <copyright file="Form1.cs" company="dyadica.co.uk">
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
// with the Kinect SDK Version 1.8 within a win form app.
// Based on and using code from http://msdn.microsoft.com/en-us/library/hh855387.aspx
// </summary

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using System.IO;
using Microsoft.Kinect;
using Microsoft.Kinect.Interop;

using Microsoft.Speech.AudioFormat;
using Microsoft.Speech.Recognition;

namespace Kinect_Speech_WF
{
    public partial class Form1 : Form
    {
        /// <summary>
        /// Active Kinect sensor.
        /// </summary>
        public static KinectSensor Sensor;

        /// <summary>
        /// The Speech Manager
        /// </summary>
        public SpeechManager SpeechManager;

        /// <summary>
        /// The path location of the grammar file to be used
        /// </summary>
        public string GrammarFile = AppDomain.CurrentDomain.BaseDirectory + "Grammar.xml";

        /// <summary>
        /// The forms constructor
        /// </summary>
        public Form1()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Event called when the form loads
        /// </summary>
        /// <param name="sender">object sending the event.</param>
        /// <param name="e">object sending the event.</param>
        private void Form1_Load(object sender, EventArgs e)
        {
            // Find a connected Kinect and try to
            // initialise it.

            InitialiseTheKinectSensor();

            // If we have a sensor then create a new
            // SpeechManager to handle incoming speech.

            if (Sensor != null)
                SpeechManager = new SpeechManager(Sensor, GrammarFile);

            // Register for speech events

            SpeechManager.SpeechEngine.SpeechRecognized +=
                SpeechEngine_SpeechRecognized;

            SpeechManager.SpeechEngine.SpeechHypothesized +=
                SpeechEngine_SpeechHypothesized;

            SpeechManager.SpeechEngine.SpeechRecognitionRejected +=
                SpeechEngine_SpeechRecognitionRejected;

        }

        /// <summary>
        /// Handler for recognized speech events.
        /// </summary>
        /// <param name="sender">object sending the event.</param>
        /// <param name="e">event arguments.</param>
        private void SpeechEngine_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            // throw new NotImplementedException();

            // Speech utterance confidence below which we 
            // treat speech as if it hadn't been heard.

            const double ConfidenceThreshold = 0.3;

            if (e.Result.Confidence >= ConfidenceThreshold)
            {
                label1.Text = e.Result.Semantics.Value.ToString();
            }
        }

        /// <summary>
        /// Handler for hypothesized speech events.
        /// </summary>
        /// <param name="sender">object sending the event.</param>
        /// <param name="e">c</param>
        private void SpeechEngine_SpeechHypothesized(object sender, SpeechHypothesizedEventArgs e)
        {
            // throw new NotImplementedException();

            System.Diagnostics.Debug.WriteLine("Speech hypothesized: " + e.Result.Text);
        }

        /// <summary>
        /// Handler for rejected speech events.
        /// </summary>
        /// <param name="sender">object sending the event.</param>
        /// <param name="e">event arguments.</param>
        private void SpeechEngine_SpeechRecognitionRejected(object sender, SpeechRecognitionRejectedEventArgs e)
        {
            // throw new NotImplementedException();

            System.Diagnostics.Debug.WriteLine("Say what?");
        }

        /// <summary>
        /// Function used to both find and initialise the 
        /// Kinect Sensor.
        /// </summary>
        private void InitialiseTheKinectSensor()
        {
            // Look through all sensors and start the first connected one.
            // This requires that a Kinect is connected at the time of app 
            // startup.

            // To make your app robust against plug/unplug, it is recommended 
            // to use KinectSensorChooser provided in Microsoft.Kinect.Toolkit 
            // (See components in Toolkit Browser).

            foreach (var potentialSensor in KinectSensor.KinectSensors)
            {
                if (potentialSensor.Status == KinectStatus.Connected)
                { Sensor = potentialSensor; break; }
            }

            // If we have found a Kinect then try and start it going.

            if (Sensor != null)
            {
                try
                {
                    Sensor.Start();
                }
                catch (IOException)
                {
                    // Some other application is 
                    // using the Kinect sensor.

                    Sensor = null;
                }
            }
        }

        /// <summary>
        /// Event called when the form begins to close
        /// </summary>
        /// <param name="sender">object sending the event.</param>
        /// <param name="e">event arguments.</param>
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            // Stop and clean up speech recognition
            // and the Kinect. 

            // This causes a hang up??

            // SpeechManager.StopRecognisingSpeech();

            // Remove events here too? I don't know
            // if this is needed or not! I think they
            // would be disposed?

            // Shutdown and clean up the Kinect

            //if (Sensor != null)
            //{
            //    Sensor.AudioSource.Stop();
            //    Sensor.Stop();
            //    Sensor = null;
            //}
        }
    }
}
