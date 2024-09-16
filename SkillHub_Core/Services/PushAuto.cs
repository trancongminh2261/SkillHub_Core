﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LMSCore.Users;

namespace LMS_Project.Services
{
    public class PushAuto
    {
        /// <summary>
        /// Tự động chạy 1 phút 1 lần
        /// </summary>
        public static void Minutely()
        {
            Task.Run(() => {
                ///Tự động tắt phòng zoom
                ZoomRoomService.AutoCloseRoom();
                NotificationInVideoService.SeenNotification();
            });
        }
        public static void Daily()
        {
            Task.Run(() =>
            {
            });
        }
        public static void Monthly()
        {
            Task.Run(() =>
            {
            });
        }
    }
}