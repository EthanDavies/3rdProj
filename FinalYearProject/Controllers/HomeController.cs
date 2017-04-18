using NAudio.Lame;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Speech.AudioFormat;
using System.Speech.Synthesis;
using System.Threading;
using System.Web;
using System.Web.Mvc;


namespace FinalYearProject.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        [Authorize(Roles = "Admin, Lecturer, Student")]
        public ActionResult Chat()
        {
            return View();
        }

        public ActionResult PlayTextArea(string text)
        {

            if (String.IsNullOrEmpty(text))
                text = "Type something in first";

            return TextToMp3(text);
        }
        public FileResult TextToMp3(string text)
        {
            //Primary memory stream for storing mp3 audio
            var mp3Stream = new MemoryStream();
            //Speech format
            var speechAudioFormatConfig = new SpeechAudioFormatInfo(samplesPerSecond: 8000, bitsPerSample: AudioBitsPerSample.Sixteen, channel: AudioChannel.Stereo);
            //Naudio's wave format used for mp3 conversion. Mirror configuration of speech config.
            var waveFormat = new WaveFormat(speechAudioFormatConfig.SamplesPerSecond, speechAudioFormatConfig.BitsPerSample, speechAudioFormatConfig.ChannelCount);
            try
            {
                //Build a voice prompt to have the voice talk slower and with an emphasis on words
                var prompt = new PromptBuilder { Culture = CultureInfo.CreateSpecificCulture("en-US") };
                prompt.StartVoice(prompt.Culture);
                prompt.StartSentence();
                prompt.StartStyle(new PromptStyle() { Emphasis = PromptEmphasis.Reduced, Rate = PromptRate.Slow });
                prompt.AppendText(text);
                prompt.EndStyle();
                prompt.EndSentence();
                prompt.EndVoice();

                //Wav stream output of converted text to speech
                using (var synthWavMs = new MemoryStream())
                {
                    //Spin off a new thread that's safe for an ASP.NET application pool.
                    var resetEvent = new ManualResetEvent(false);
                    ThreadPool.QueueUserWorkItem(arg =>
                    {
                        try
                        {
                            //initialize a voice with standard settings
                            var siteSpeechSynth = new SpeechSynthesizer();
                            //Set memory stream and audio format to speech synthesizer
                            siteSpeechSynth.SetOutputToAudioStream(synthWavMs, speechAudioFormatConfig);
                            //build a speech prompt
                            siteSpeechSynth.Speak(prompt);
                        }
                        catch (Exception ex)
                        {
                            //This is here to diagnostic any issues with the conversion process. It can be removed after testing.
                            Response.AddHeader("EXCEPTION", ex.GetBaseException().ToString());
                        }
                        finally
                        {
                            resetEvent.Set();//end of thread
                        }
                    });
                    //Wait until thread catches up with us
                    WaitHandle.WaitAll(new WaitHandle[] { resetEvent });
                    //Estimated bitrate
                    var bitRate = (speechAudioFormatConfig.AverageBytesPerSecond * 8);
                    //Set at starting position
                    synthWavMs.Position = 0;
                    //Be sure to have a bin folder with lame dll files in there.  They also need to be loaded on application start up via Global.asax file
                    using (var mp3FileWriter = new LameMP3FileWriter(outStream: mp3Stream, format: waveFormat, bitRate: bitRate))
                        synthWavMs.CopyTo(mp3FileWriter);
                }
            }
            catch (Exception ex)
            {
                Response.AddHeader("EXCEPTION", ex.GetBaseException().ToString());
            }
            finally
            {
                //Set no cache on this file
                Response.Cache.SetExpires(DateTime.UtcNow.AddMinutes(-1));
                Response.Cache.SetCacheability(HttpCacheability.NoCache);
                Response.Cache.SetNoStore();
                //required for chrome and safari
                Response.AppendHeader("Accept-Ranges", "bytes");
                //Write the byte length of mp3 to the client
                Response.AddHeader("Content-Length", mp3Stream.Length.ToString(CultureInfo.InvariantCulture));
            }
            //return the converted wav to mp3 stream to a byte array for a file download
            return File(mp3Stream.ToArray(), "audio/mp3");
        }

        
    }
}