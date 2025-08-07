using System;
using System.Windows;
using System.Windows.Input;
using Common.Logging;
using GalaSoft.MvvmLight;
using Microsoft.Win32;
using ScanPlayerWpf.Controls;
using ScanPlayerWpf.Models.StandardPrinters;
using ScanPlayerWpf.Rendering;

namespace ScanPlayerWpf.Models
{
    public sealed class Workspace : ObservableObject
    {
        private static readonly ILog log = LogManager.GetLogger<Workspace>();

        public Workspace()
        {
            SceneOptions = new SceneOptions();
            ProjectLoader = new ProjectLoader();
            Printer = new FormUp350(2);
        }

        internal Window MainWindow { get; set; }

        private IProject project;
        public IProject Project 
        {
            get => project;
            set
            {
                if (Set(ref project, value))
                    RaisePropertyChanged(nameof(ProjectFileName));
            }
        }

        private IPrinterDefinition printer;
        public IPrinterDefinition Printer
        {
            get => printer;
            set => Set(ref printer, value);
        }

        private IDrawingProgram drawingProgram;
        public IDrawingProgram DrawingProgram
        {
            get => drawingProgram;
            set => Set(ref drawingProgram, value);
        }

        public string ProjectFileName => project?.FileName;

        public SceneOptions SceneOptions { get; set; }
        public IProjectLoader ProjectLoader { get; }
        
        public void LoadProject()
        {
            var dialog = new OpenFileDialog
            {
                Filter = ProjectLoader.FileFilter
            };

            if (dialog.ShowDialog(MainWindow) == true)
                LoadProject(dialog.FileName);
        }

        public void LoadProject(string filename)
        {
            using (new CursorScope(MainWindow, Cursors.Wait))
            {
                log.Info($"Loading project {filename}");
                try
                {
                    if (!UnloadProject()) throw new InvalidOperationException(
                        "Cannot load a project when one is already loaded");

                    // Let's update the project loader's current printer
                    ProjectLoader.CurrentPrinter = Printer;
                    Project = ProjectLoader.LoadProject(filename);
                    log.Info($"Loaded project {filename}");
                }
                catch (Exception ex)
                {
                    log.Error($"Could not load project {filename}: {ex.Message}", ex);
                }

                try
                {
                    DrawingProgram = Project.Translate();
                }
                catch (Exception ex)
                {
                    log.Error($"Could not translate project {filename} to drawing instructions: {ex.Message}", ex);
                }
            }
        }

        public bool UnloadProject()
        {
            if (Project == null) return true;

            var filename = Project?.FileName ?? "";
            log.Info($"Unloading project {filename}");
            try
            {
                // Clean-up
                DrawingProgram = null;
                Project = null;
                log.Info($"Unloaded project {filename}");
                return true;
            }
            catch (Exception ex)
            {
                log.Error($"Could not unload project file {filename}: {ex.Message}", ex);
                return false;
            }
        }
    }
}
