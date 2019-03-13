using System;
using System.Collections.Generic;
using System.IO;
using System.Timers;
using Grasshopper.Kernel;

namespace ReadFile
{
    public class ReadFileComponent : GH_Component
    {
        private int _pathIn;
        private int _contentOut;

        private readonly List<string> _filePaths = new List<string>();
        private DateTime _lastReadTime = DateTime.MinValue;
        private Timer _timer;

        public ReadFileComponent()
            : base("Read File", "Read File",
                "Read File",
                "Params", "Util")
        {
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("d417ad8d-512a-46e6-bbc5-27b3eb35b36c"); }
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            _pathIn = pManager.AddTextParameter("File path", "Path", "Absolute file path", GH_ParamAccess.list);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            _contentOut = pManager.AddTextParameter("File content", "Content", "File content", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess da)
        {
            if (_timer == null)
            {
                _timer = new Timer
                {
                    Enabled = true,
                    Interval = (1000),
                    AutoReset = true,
                };
                _timer.Elapsed += TimerOnElapsed;
            }

            var filePaths = new List<string>();
            da.GetDataList(_pathIn, filePaths);
            if (!new HashSet<string>(filePaths).SetEquals(_filePaths))
            {
                _lastReadTime = DateTime.MinValue;
            }

            _filePaths.Clear();
            _filePaths.AddRange(filePaths);

            if (ShouldReread())
            {
                var content = new List<string>();
                foreach (var filePath in _filePaths)
                {
                    try
                    {
                        var text = File.ReadAllText(filePath);
                        content.Add(text);
                    }
                    catch
                    {
                        content.Add("");
                    }
                }

                da.SetDataList(_contentOut, content);
                _lastReadTime = DateTime.Now;
            }
        }


        private void TimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            if (ShouldReread())
            {
                ExpireSolution(true);
            }
        }

        private bool ShouldReread()
        {
            if (_filePaths.Count == 0) return false;
// ReSharper disable once ForCanBeConvertedToForeach
// ReSharper disable once LoopCanBeConvertedToQuery
            for (var i = 0; i < _filePaths.Count; i++)
            {
                var filePath = _filePaths[i];
                if (!File.Exists(filePath)) continue;
                if (File.GetLastWriteTime(filePath) > _lastReadTime) return true;
            }

            return false;
        }
    }
}