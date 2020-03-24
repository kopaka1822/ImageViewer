using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.AvalonEdit.Document;
using ImageViewer.Models;

namespace ImageViewer.UtilityEx
{
    public class PathManager
    {
        private string directory = null;

        public string Directory
        {
            get => directory;
            private set
            {
                if (value == null) return;
                directory = value;
            }
        }

        private string filename = null;

        public string Filename
        {
            get => filename;
            private set
            {
                if (value == null) return;
                filename = value;
            }
        }

        private string extension = null;

        public string Extension
        {
            get => extension;
            set
            {
                if (value == null) return;
                if (value.StartsWith("."))
                    value = value.Substring(1);
                extension = value;
            }
        }

        /// <summary>
        /// updates direction and extension if not already set.
        /// always updates filename.
        /// </summary>
        /// <param name="fallbackFilename"></param>
        public void InitFromFilename(string fallbackFilename)
        {
            if (fallbackFilename == null) return;

            // update dir if not set
            if (Directory == null)
            {
                Directory = System.IO.Path.GetDirectoryName(fallbackFilename);
            }

            // always update filename
            Filename = System.IO.Path.GetFileNameWithoutExtension(fallbackFilename);

            // update extension if not set
            if (Extension == null)
            {
                Extension = System.IO.Path.GetExtension(fallbackFilename);
            }
        }

        /// <summary>
        /// updates directory, filename and extension
        /// </summary>
        /// <param name="lastFilename"></param>
        /// <param name="updateExtension"></param>
        /// <param name="updateFilename"></param>
        /// <param name="updateDirectory"></param>
        public void UpdateFromFilename(
            string lastFilename, 
            bool updateExtension = true, 
            bool updateFilename = true, 
            bool updateDirectory = true)
        {
            if (lastFilename == null) return;
            if(updateDirectory)
                Directory = System.IO.Path.GetDirectoryName(lastFilename);
            if(updateFilename)
                Filename = System.IO.Path.GetFileNameWithoutExtension(lastFilename);
            if(updateExtension)
                Extension = System.IO.Path.GetExtension(lastFilename);
        }

        /// <summary>
        /// calls InitFromFilename with parameters of the first used equation.
        /// </summary>
        /// <param name="models"></param>
        /// <returns>true if a valid first image existed</returns>
        public bool InitFromEquations(ImageFramework.Model.Models models)
        {
            var id = models.GetFirstEnabledPipeline();
            var pipe = models.Pipelines[id];

            var firstImageId = pipe.Color.FirstImageId;
            if (!pipe.Color.HasImages)
                firstImageId = pipe.Alpha.FirstImageId;

            if (firstImageId >= models.Images.NumImages) return false;
            if (!models.Images.Images[firstImageId].IsFile) return false;

            var firstImageName = models.Images.Images[firstImageId].Filename;
            InitFromFilename(firstImageName);

            return true;
        }

    }
}
