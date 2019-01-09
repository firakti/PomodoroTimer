﻿using PomodoroTimer.Enums;
using PomodoroTimer.Models;
using PomodoroTimer.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PomodoroTimer
{

    public class PomodoroChangedEventArgs : EventArgs
    {
        public bool isCanceled { get; set; } = false;
        public PomodoroState ComplatedState { get; set; }
        public PomodoroState NextState { get; set; }
        public PomdoroStatus PomodoroStatus { get; internal set; }
    }

    public class PomodoroTimerTickEventArgs : EventArgs
    {
        public PomdoroStatus TimerInfo { get; set; }
    }

    public class UserTaskModifiedEventArgs : EventArgs
    {
        public UserTask UserTask { get; set; }
    }
    public class AppResumedEventArgs : EventArgs
    {
        public PomdoroStatus AppState { get; set; }
    }
    public delegate void UserTaskModifiedEventHandler(object sender, UserTaskModifiedEventArgs eventArgs);
    public delegate void TimerFinishedEventHandler(object sender, PomodoroChangedEventArgs eventArgs);
    public delegate void TimerTickEventHandler(object sender, PomodoroTimerTickEventArgs eventArgs);
    public delegate void AppResumedEventHandler(object sender, AppResumedEventArgs eventArgs);

    public interface IAppService
    {
        AplicationUser User { get; set; }
        UserTask ActiveTask { get; set; }
        List<UserTask> UserTasks { get; set; }
        AppSettings AppSettings { get; set; }
        PomodoroSettings PomodoroSettings { get; set; }
        PomodoroSession CurrentSession { get; set; }
        PomdoroStatus PomodoroStatus { get; }

        event TimerFinishedEventHandler TimerFinishedEvent;
        event TimerTickEventHandler TimerTickEvent;
        event UserTaskModifiedEventHandler UserTaskModifiedEvent;
        event UserTaskModifiedEventHandler UserTaskRemovedEvent;
        event AppResumedEventHandler AppResumedEvent;
        
        PomdoroStatus StartPomodoro();
        PomdoroStatus PausePomodoro();
        PomdoroStatus StopPomodoro();

        void DisableNotification();
        void EnableNotification();

        void SetActiveTask(UserTask selectedUserTask);

        void ClearStatistics();

        Task<bool> AddNewUserTask(UserTask userTask);
        Task<bool> RemoveUserTask(UserTask userTask);
        Task<bool> SaveSettingsAsync(AppSettings settings);
        List<TaskStatistic> GetStatisticData(DateTime startTime, DateTime finishTime);

        void Export(DateTime startTime, DateTime finishTime);
        void OnResume();
        void OnSleep();
        void OnDestroy();
        void SetTimerInfo(PomdoroStatus timerInfo);
    }
}