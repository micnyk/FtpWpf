using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FtpWpf.FileSystemModel;

namespace FtpWpf
{
    public class ActionEventArgs : EventArgs
    {
        public enum ActionType
        {
            ListDirectory,
            DownloadFile,
            UploadFile,
            Rename,
            Delete,
            New
        }

        public enum ActionStatus
        {
            Started,
            Succeed,
            Failed
        }

        public ActionStatus Status { get; set; }
        public ActionType Action { get; set; }
        public Item Target { get; set; }

        public static ActionEventArgs ListStarted(Item item)
        {
            return new ActionEventArgs
            {
                Action = ActionType.ListDirectory,
                Status = ActionStatus.Started,
                Target = item
            };
        }

        public static ActionEventArgs ListSucceed(Item item)
        {
            return new ActionEventArgs
            {
                Action = ActionType.ListDirectory,
                Status = ActionStatus.Succeed,
                Target = item
            };
        }

        public static ActionEventArgs ListFailed(Item item)
        {
            return new ActionEventArgs
            {
                Action = ActionType.ListDirectory,
                Status = ActionStatus.Failed,
                Target = item
            };
        }

        public static ActionEventArgs DownloadStarted(Item item)
        {
            return new ActionEventArgs
            {
                Action = ActionType.DownloadFile,
                Status = ActionStatus.Started,
                Target = item
            };
        }

        public static ActionEventArgs DownloadSucceed(Item item)
        {
            return new ActionEventArgs
            {
                Action = ActionType.DownloadFile,
                Status = ActionStatus.Succeed,
                Target = item
            };
        }

        public static ActionEventArgs DownloadFailed(Item item)
        {
            return new ActionEventArgs
            {
                Action = ActionType.DownloadFile,
                Status = ActionStatus.Failed,
                Target = item
            };
        }

        public static ActionEventArgs RenameStarted(Item item)
        {
            return new ActionEventArgs
            {
                Action = ActionType.Rename,
                Status = ActionStatus.Started,
                Target = item
            };
        }

        public static ActionEventArgs RenameSucceed(Item item)
        {
            return new ActionEventArgs
            {
                Action = ActionType.Rename,
                Status = ActionStatus.Succeed,
                Target = item
            };
        }

        public static ActionEventArgs RenameFailed(Item item)
        {
            return new ActionEventArgs
            {
                Action = ActionType.Rename,
                Status = ActionStatus.Failed,
                Target = item
            };
        }

        public static ActionEventArgs DeleteStarted(Item item)
        {
            return new ActionEventArgs
            {
                Action = ActionType.Delete,
                Status = ActionStatus.Started,
                Target = item
            };
        }

        public static ActionEventArgs DeleteSucceed(Item item)
        {
            return new ActionEventArgs
            {
                Action = ActionType.Delete,
                Status = ActionStatus.Succeed,
                Target = item
            };
        }

        public static ActionEventArgs DeleteFailed(Item item)
        {
            return new ActionEventArgs
            {
                Action = ActionType.Delete,
                Status = ActionStatus.Failed,
                Target = item
            };
        }

        public static ActionEventArgs NewStarted(Item item)
        {
            return new ActionEventArgs
            {
                Action = ActionType.New,
                Status = ActionStatus.Started,
                Target = item
            };
        }

        public static ActionEventArgs NewSucceed(Item item)
        {
            return new ActionEventArgs
            {
                Action = ActionType.New,
                Status = ActionStatus.Succeed,
                Target = item
            };
        }

        public static ActionEventArgs NewFailed(Item item)
        {
            return new ActionEventArgs
            {
                Action = ActionType.New,
                Status = ActionStatus.Failed,
                Target = item
            };
        }

        public static ActionEventArgs UploadStarted(Item item)
        {
            return new ActionEventArgs
            {
                Action = ActionType.UploadFile,
                Status = ActionStatus.Started,
                Target = item
            };
        }

        public static ActionEventArgs UploadSucceed(Item item)
        {
            return new ActionEventArgs
            {
                Action = ActionType.UploadFile,
                Status = ActionStatus.Succeed,
                Target = item
            };
        }

        public static ActionEventArgs UploadFailed(Item item)
        {
            return new ActionEventArgs
            {
                Action = ActionType.UploadFile,
                Status = ActionStatus.Failed,
                Target = item
            };
        }
    }
}
