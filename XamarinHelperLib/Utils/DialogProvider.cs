﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace XamarinHelpers.Utils
{
    public class DialogProvider : IDialogProvider
    {
        private readonly Page _page;

        public DialogProvider(Page page)
        {
            _page = page;
        }

        public Task DisplayAlert(string title, string message, string cancel)
        {
            return _page.DisplayAlert(title, message, cancel);
        }

        public async Task<bool> DisplayAlert(string title, string message, string accept, string cancel)
        {
            return await _page.DisplayAlert(title, message, accept, cancel);
        }

        public async Task<string> DisplayActionSheet(string title, string cancel, string destruction, params string[] buttons)
        {
            return await _page.DisplayActionSheet(title, cancel, destruction, buttons);
        }
    }
}
