using Accord.Video.FFMPEG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Whorl
{
    public class VideoOps
    {
        public string FileName { get; set; }
        public bool IsRecording { get; set; }
        private VideoFileWriter aviWriter;

        public void OpenWriter(string fileName, Size frameSize, int framesPerSecond = 10)
        {
            this.FileName = fileName;
            aviWriter = new VideoFileWriter();
            aviWriter.Open(fileName, frameSize.Width, frameSize.Height, 
                           framesPerSecond, VideoCodec.WMV2);
        }

        public void CloseWriter()
        {
            if (aviWriter != null)
            {
                aviWriter.Close();
                aviWriter = null;
            }
        }

        public void AddFrame(Bitmap bitmap)
        {
            aviWriter.WriteVideoFrame(bitmap);
        }
    }
}
