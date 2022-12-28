using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using System.Collections.ObjectModel;

namespace RoofFrameSchedule
{
    class viewModelRFS : BindableBase
    {
        private ObservableCollection<MyFrameType> _colFrames;
        private MyFrameType _selectedFrame;
        private ObservableCollection<MyFrameType> _colSelectedFrames;
        private MyFrameType _selectedFrameFromSelected;
        private string _nameDraftingView;
        private Dictionary<int, string> _dictDraftingViews;
        private int _selectedDraftingView;
        private bool _isSelectedView;

        private modelRFS _revitModel;

        private ViewRFS _view;

        public ObservableCollection<MyFrameType> ColFrames
        {
            get { return _colFrames; }
            set
            {
                SetProperty(ref _colFrames, value);
            }
        }        
        public MyFrameType SelectedFrame
        {
            get { return _selectedFrame; }
            set
            {
                SetProperty(ref _selectedFrame, value);
            }
        }
        public ObservableCollection<MyFrameType> ColSelectedFrames
        {
            get { return _colSelectedFrames; }
            set
            {
                SetProperty(ref _colSelectedFrames, value);
            }
        }        
        public MyFrameType SelectedFrameFromSelected
        {
            get { return _selectedFrameFromSelected; }
            set
            {
                SetProperty(ref _selectedFrameFromSelected, value);
            }
        }
        public string NameDraftingView
        {
            get
            {
                return _nameDraftingView;
            }
            set
            {
                SetProperty(ref _nameDraftingView, value);
            }
        }
        public Dictionary<int, string> DictDraftingViews
        {
            get
            {
                return _dictDraftingViews;
            }
            set
            {
                SetProperty(ref _dictDraftingViews, value);
            }
        }
        public int SelectedDraftingView
        {
            get { return _selectedDraftingView; }
            set
            {
                SetProperty(ref _selectedDraftingView, value);
            }
        }
        public bool IsSelectedView
        {
            get
            {
                return _isSelectedView;
            }
            set
            {
                _isSelectedView = value;
                SetProperty(ref _isSelectedView, value);
            }
        }
        public ICommand AddFrame
        {
            get
            {
                return new DelegateCommand(AddFrameAction);
            }
        }
        private void AddFrameAction()
        {
            try
            {
                if (SelectedFrame != null && ColSelectedFrames.Contains(SelectedFrame) == false)
                {
                    ColSelectedFrames.Add(SelectedFrame);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + ex.StackTrace);
            }            
        }
        public ICommand RemoveFrame
        {
            get { return new DelegateCommand(RemoveFrameAction); }
        }
        private void RemoveFrameAction()
        {
            try
            {
                if (SelectedFrameFromSelected != null)
                {
                    ColSelectedFrames.Remove(SelectedFrameFromSelected);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + ex.StackTrace);
            }
        }
        public ICommand MoveUp
        {
            get { return new DelegateCommand(MoveUpAction); }
        }
        private void MoveUpAction()
        {
            int newInd = ColSelectedFrames.IndexOf(SelectedFrameFromSelected) - 1;
            if (SelectedFrameFromSelected != null && newInd >= 0)
            {
                ColSelectedFrames.Move(ColSelectedFrames.IndexOf(SelectedFrameFromSelected), newInd);
            }
        }
        public ICommand MoveDown
        {
            get { return new DelegateCommand(MoveDownAction); }
        }
        private void MoveDownAction()
        {
            int newInd = ColSelectedFrames.IndexOf(SelectedFrameFromSelected) + 1;
            if (SelectedFrameFromSelected != null && newInd <= ColSelectedFrames.Count - 1)
            {
                ColSelectedFrames.Move(ColSelectedFrames.IndexOf(SelectedFrameFromSelected), newInd);
            }
        }

        public ICommand CreateSchedule
        {
            get { return new DelegateCommand(CreateScheduleAction); }
        }
        private void CreateScheduleAction()
        {
            if (ColSelectedFrames.Count != 0)
            {
                bool res = RevitModel.Create(ColSelectedFrames);
                View.DialogResult = res;
                //View.Close();
            }            
        }
        internal modelRFS RevitModel
        {
            get
            {
                return _revitModel;
            }
            set
            {
                _revitModel = value;
            }
        }
        internal ViewRFS View
        {
            get
            {
                return _view;
            }
            set
            {
                _view = value;
            }
        }
        public viewModelRFS()
        {

        }
    }
}
