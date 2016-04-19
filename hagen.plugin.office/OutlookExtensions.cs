// Copyright (c) 2016, Andreas Grimme

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sidi.Extensions;
using System.Runtime.InteropServices;
using Microsoft.Office.Interop.Outlook;

namespace hagen.plugin.office
{
    public static class OutlookExtensions
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static Application ProvideApplication()
        {
            try
            {
                var instance = Marshal.GetActiveObject("Outlook.Application") as Microsoft.Office.Interop.Outlook.Application;
                if (instance != null)
                {
                    return instance;
                }
            }
            catch
            {
            }

            return new Microsoft.Office.Interop.Outlook.Application();
        }

        public static TaskItem CreateTaskItem(this Application app)
        {
            var task = (Microsoft.Office.Interop.Outlook.TaskItem)app.CreateItem(Microsoft.Office.Interop.Outlook.OlItemType.olTaskItem);
            return task;
        }
    }
}
