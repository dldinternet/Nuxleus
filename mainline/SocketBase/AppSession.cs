﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Authentication;
using System.Text;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Config;
using SuperSocket.Common;

namespace SuperSocket.SocketBase
{
    public abstract class AppSession<TAppSession, TCommandInfo> : IAppSession, IAppSession<TAppSession, TCommandInfo>
        where TAppSession : IAppSession, IAppSession<TAppSession, TCommandInfo>, new()
        where TCommandInfo : ICommandInfo
    {
        public virtual IAppServer<TAppSession, TCommandInfo> AppServer { get; private set; }

        protected void Initialize(IAppServer<TAppSession, TCommandInfo> appServer, ISocketSession socketSession, SocketContext context)
        {
            Context = context;
            AppServer = appServer;
            SocketSession = socketSession;
            SocketSession.Closed += new EventHandler<SocketSessionClosedEventArgs>(SocketSession_Closed);
            SessionID = socketSession.SessionID;
            IdentityKey = socketSession.IdentityKey;
            OnInit();
        }

        public virtual void Initialize(IAppServer<TAppSession, TCommandInfo> appServer, ISocketSession socketSession)
        {
            Initialize(appServer, socketSession, new SocketContext());
        }

        void SocketSession_Closed(object sender, SocketSessionClosedEventArgs e)
        {
            OnClosed();
        }

        protected abstract void OnClosed();

        protected virtual void OnInit()
        {
            this.StartTime = DateTime.Now;
        }

        public virtual void StartSession()
        {

        }

        public abstract void HandleExceptionalError(Exception e);

        public void ExecuteCommand(TAppSession session, TCommandInfo cmdInfo)
        {
            ////Header
            //if (string.IsNullOrEmpty(Context.PrevCommand) && AppServer.HeaderParser != null)
            //{
            //    AppServer.HeaderParser.ExecuteParser(AppSession, commandLine);
            //    AppSession.Context.PrevCommand = "HEADER";
            //    return;
            //}

            ICommand<TAppSession, TCommandInfo> command = AppServer.GetCommandByName(cmdInfo.CommandKey);

            if (command != null)
            {
                Context.CurrentCommand = cmdInfo.CommandKey;
                command.ExecuteCommand(session, cmdInfo);
                Context.PrevCommand = cmdInfo.CommandKey;
                if (AppServer.Config.LogCommand)
                    LogUtil.LogInfo(AppServer, string.Format("Command - {0} - {1}", IdentityKey, cmdInfo.CommandKey));
            }
            else
            {
                HandleUnknownCommand(cmdInfo);
            }

            LastActiveTime = DateTime.Now;
        }

        public virtual void HandleUnknownCommand(TCommandInfo cmdInfo)
        {
            SendResponse("Unknown command: " + cmdInfo.CommandKey);
        }

        public virtual SocketContext Context { get; private set; }

        public SslProtocols SecureProtocol
        {
            get { return SocketSession.SecureProtocol; }
            set { SocketSession.SecureProtocol = value; }
        }

        public IPEndPoint LocalEndPoint
        {
            get { return SocketSession.LocalEndPoint; }
        }

        public IPEndPoint RemoteEndPoint
        {
            get { return SocketSession.RemoteEndPoint; }
        }

        public DateTime LastActiveTime { get; set; }

        public DateTime StartTime { get; private set; }

        public string SessionID { get; private set; }

        public string IdentityKey { get; private set; }

        public ISocketSession SocketSession { get; private set; }

        public IServerConfig Config
        {
            get { return AppServer.Config; }
        }

        public virtual void Close()
        {
            this.SocketSession.Close();
            OnClosed();
        }

        public virtual void SendResponse(string message)
        {
            SocketSession.SendResponse(Context, message);
        }

        public virtual void SendResponse(string message, params object[] paramValues)
        {
            SocketSession.SendResponse(Context, string.Format(message, paramValues));
        }
    }

    public abstract class AppSession<TAppSession> : AppSession<TAppSession, StringCommandInfo>
        where TAppSession : IAppSession, IAppSession<TAppSession, StringCommandInfo>, new()
    {
        protected virtual string ProcessSendingMessage(string rawMessage)
        {
            if (AppServer.Config.Mode == SocketMode.Udp)
                return rawMessage;

            if (string.IsNullOrEmpty(rawMessage) || !rawMessage.EndsWith(Environment.NewLine))
                return rawMessage + Environment.NewLine;
            else
                return rawMessage;
        }

        public override void SendResponse(string message)
        {
            base.SendResponse(ProcessSendingMessage(message));
        }

        public override void SendResponse(string message, params object[] paramValues)
        {
            base.SendResponse(ProcessSendingMessage(message), paramValues);
        }
    }
}
