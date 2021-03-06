﻿using Microcharts;
using Plugin.LocalNotifications;
using PomodoroTimer.Enums;
using PomodoroTimer.Messaging;
using PomodoroTimer.Models;
using PomodoroTimer.Services;
using PomodoroTimer.ViewModels.ObjectViewModel;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;
using XamarinHelpers.MVVM;
using XamarinHelpers.Utils;

namespace PomodoroTimer.ViewModels
{
    public class HomeViewModel : PageViewModel
    {
        #region fields
        private PomodoroTimerState _timerInfo;
        private string _taskName;
        private UserTask _activeTask;
        public Chart _chart;
        private ObservableCollection<UserTaskViewModel> _userTask;
        private float _tick = 0;
        private int _tickCount;
        #endregion

        #region props

        private IAppService AppService { get; set; }
        private UITimer UiTimer { get; set; }
        public ICommand SetTimerStatus { get; set; }
        public ICommand StopTimer { get; set; }
        public ICommand SetTask { get; set; }
        public ICommand ChangeTask { get; set; }


        public bool IsTimerRunning
        {
            get { return TimerInfo.TimerState == TimerState.Running; }
        }
        public bool IsPomodoro
        {
            get { return TimerInfo.PomodoroState == PomodoroState.Pomodoro; }
        }

        public int TickCount
        {
            get { return _tickCount; }
            set { SetProperty(ref _tickCount, value); }
        }
        public float Tick
        {
            get { return _tick; }
            set { SetProperty(ref _tick, value); }
        }

        public string TaskName
        {
            get { return _taskName; }
            set { SetProperty(ref _taskName, value); }
        }

        public string RemainingTime
        {
            get
            {
                string value = "";
                if (RemainingTimeValue.Hours != 0)
                    value += RemainingTimeValue.Hours;

                if (RemainingTimeValue.Minutes < 10)
                    value += "0";

                value += RemainingTimeValue.Minutes + ":";

                if (RemainingTimeValue.Seconds < 10)
                    value += "0";

                value += RemainingTimeValue.Seconds;
                return value;
            }
        }

        public TimeSpan RemainingTimeValue
        {
            get
            {
                return TimerInfo.RemainingTime;
            }
        }

        public PomodoroTimerState TimerInfo
        {
            get { return _timerInfo; }
            set
            {
                SetProperty(ref _timerInfo, value);
                OnPropertyChanged("RemainingTime");
                OnPropertyChanged("IsPomodoro");
                OnPropertyChanged("IsTimerRunning");
            }
        }

        public UserTask ActiveTask
        {
            get { return _activeTask; }
            set
            {
                SetProperty(ref _activeTask, value);
                TaskName = ActiveTask.TaskName;
            }
        }

        public ObservableCollection<UserTaskViewModel> UserTasks
        {
            get
            {
                return _userTask;
            }
            set
            {
                SetProperty(ref _userTask, value);
            }
        }
        #endregion


        public HomeViewModel(IAppService appService)
        {
            TickCount = 90;

            AppService = appService;

            ActiveTask = AppService.ActiveTask;
            UserTasks = new ObservableCollection<UserTaskViewModel>(AppService.UserTasks.Select(x => new UserTaskViewModel(x)));

            AppService.UserTaskRemovedEvent += OnUserTaskRemoved;
            AppService.TimerFinishedEvent += OnTimerFinished;
            AppService.UserTaskModifiedEvent += OnUserTaskUpdate;
            AppService.AppResumedEvent += OnAppResumed;
            AppService.PomodoroTimerStateChangedEvent += OnPomodoroStateChanged;

            LoadState(appService.PomodoroStatus);

            ChangeTask = new Command(
                execute: async (o) =>
                {
                    if (o is UserTaskViewModel userTaskViewModel)
                    {
                        if (userTaskViewModel.Id == ActiveTask.Id)
                            return;

                        if (IsPomodoro && IsTimerRunning)
                        {
                            var displayAlert = new DialogProvider(Page);
                            var changeTask = await displayAlert.DisplayAlert("Change Task", "Pomodoro will be cancelled. Did you want to continue", "ok", "cancel");
                            if (!changeTask)
                                return;
                        }

                        StopTimer.Execute(null);
                        ActiveTask = userTaskViewModel.UserTask;
                        AppService.SetActiveTask(userTaskViewModel.UserTask);

                    }
                }
            );

            SetTimerStatus = new Command(
                execute: () =>
                {
                    if (IsPomodoro && IsTimerRunning)
                    {
                        TimerInfo = AppService.PausePomodoro();
                        //StopTimerTick();
                    }
                    else
                    {
                        TimerInfo = AppService.StartPomodoro();
                        //StartTimerTick();
                    }
                }
            );

            StopTimer = new Command(
                execute: () =>
                {
                    //TimerInfo.TimerState = TimerState.Stoped;
                    //TimerInfo.PomodoroState = PomodoroState.Ready;

                    //RemainingTimeValue = TimeSpan.Zero;
                    //Tick = 0;

                    //OnPropertyChanged("TimerInfo");
                    Tick = 0;
                    AppService.StopPomodoro();

                    //StopTimerTick();
                }
            );
        }

        private void OnPomodoroStateChanged(object sender, PomodoroTimerStateChangedEventArgs eventArgs)
        {
            LoadState(eventArgs.NewState);
        }

        private void OnAppResumed(object sender, AppResumedEventArgs eventArgs)
        {
            LoadState(eventArgs.AppState);
        }

        public void LoadState(PomodoroTimerState state)
        {
            if (state == null)
                return;

            TimerInfo = new PomodoroTimerState()
            {
                PomodoroState = state.PomodoroState,
                RunTime = state.RunTime,
                TimerState = state.TimerState,
                StartTime = state.StartTime,
                RemainingRunTime = state.RemainingRunTime,
            };

            UpdateChart(RemainingTimeValue, TimeSpan.Zero, TimerInfo.RunTime);

            if (IsTimerRunning)
            {
                StartTimerTick();
            }
            else
            {
                StopTimerTick();
            }
        }

        private void StartTimerTick()
        {
            StopTimerTick();
            UiTimer = new UITimer();
            UiTimer.TimerTickEvent += OnUITimerTick;
            UiTimer.Start(TimerInfo.RemainingTime, TimeSpan.FromSeconds(1));
        }

        private void OnUITimerTick(object sender, UITimerTickEventArgs eventArgs)
        {
            AppService.SetNotification(TimerInfo);
            UpdateChart(RemainingTimeValue, TimeSpan.Zero, TimerInfo.RunTime);
            OnPropertyChanged("RemainingTime");
        }

        private void StopTimerTick()
        {
            UiTimer?.Stop();
        }
        private void OnUserTaskRemoved(object sender, UserTaskModifiedEventArgs args)
        {
            var removedTask = UserTasks.FirstOrDefault(x => x.Id == args.UserTask?.Id);
            if (removedTask != null)
            {
                UserTasks.Remove(removedTask);
            }
        }
        private void OnUserTaskUpdate(object sender, UserTaskModifiedEventArgs args)
        {
            bool contain = false;

            for (int i = 0; i < UserTasks.Count; i++)
            {
                if (UserTasks[i].Id == args.UserTask?.Id)
                {
                    UserTasks[i] = new UserTaskViewModel(args.UserTask);
                    contain = true;
                    break;
                }
            }
            if (!contain)
            {
                UserTasks.Add(new UserTaskViewModel(args.UserTask));
            }
        }

        private void OnTimerFinished(object sender, PomodoroChangedEventArgs args)
        {
            StopTimerTick();

            Device.BeginInvokeOnMainThread(() =>
            {
                TimerInfo = new PomodoroTimerState();
                Tick = 0;
            });
        }

        private void UpdateChart(TimeSpan value, TimeSpan min, TimeSpan max)
        {
            if (max == null || value == null || min == null)
            {
                Tick = 0;
            }
            else
            {
                double factor = TickCount / (max.TotalMilliseconds - min.TotalMilliseconds);
                Tick = (float)((max.TotalMilliseconds - value.TotalMilliseconds) * factor);
            }
        }
    }
}