using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using MvvmCross;
using MvvmCross.Base;
using MvvmCross.Commands;
using MvvmCross.Logging;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;
using PCController.Core.Managers;
using PCController.DataAccess;
using PCController.DataAccess.Models;
using ThreeByteLibrary.Dotnet;
using ThreeByteLibrary.Dotnet.NetworkUtils;
using LogModel = PCController.Core.Models.LogModel;

namespace PCController.Core.ViewModels
{
    public class SCSTesterViewModel : MvxNavigationViewModel<WindowChildParam>
    {
        private readonly Stopwatch stopwatch;

        public Stopwatch NetStopwatch = new Stopwatch();

        private ObservableCollection<UdpSenderModel> udpSender = new();

        private WindowChildParam _param;

        public SCSTesterViewModel(IMvxLogProvider logProvider, IMvxNavigationService navigationService) :
            base(logProvider, navigationService)
        {
            
            this.SendUdpCommand = new MvxCommand(this.AddUdpFrame);
            this.IPBoxTextChangeCommand = new MvxCommand(this.GetIpSuggestionsFromDb);

            stopwatch = new Stopwatch();

            GetIpSuggestionsFromDb();
        }

        public int ParentNo => _param.ParentNo;
        public string Text => $"I'm No.{_param.ChildNo}. My parent is No.{_param.ParentNo}";

        public override void Prepare(WindowChildParam param) => _param = param;

        public IMvxCommand SendUdpCommand { get; set; }

        public ObservableCollection<UdpSenderModel> UdpSender
        {
            get
            {
                return this.udpSender;
            }
            set
            {
                this.SetProperty(ref this.udpSender, value);
            }
        }

        public void AddUdpFrame()
        {
            try
            {
                //log this
                Console.WriteLine("creating the UPD link");
                ThreeByteLibrary.Dotnet.NetworkUtils.AsyncUdpLink link = new AsyncUdpLink(IPAddress, PortNum, LocalPortNum);

                string inputFrame = String.Empty;
                //byte[] bytesToSend = Parse(inputFrame);
                
                UdpSenderModel u = new UdpSenderModel { IPAddress = this.IPAddress, PortNum = this.PortNum, MessageSent = this.MessageSent };
                this.UdpSender.Add(u);

                //link.SendMessage(bytesToSend);
                link.Dispose();
            }
            catch (Exception e)
            {
                //Log this
                Console.WriteLine(e);
                //throw;
            }


        }

        public bool CanSendMsg => this.IPAddress?.Length > 0 && this.MessageSent?.Length > 0;


        private string iPAddress;

        public string IPAddress
        {
            get { return iPAddress; }
            set
            {
                this.SetProperty(ref iPAddress, value);
                RaisePropertyChanged(() => CanSendMsg);
                RaisePropertyChanged(() => FrameToSend);
            }
        }

        private string messageSent;

        public string MessageSent
        {
            get { return messageSent; }
            set
            {
                this.SetProperty(ref messageSent, value);
                RaisePropertyChanged(() => CanSendMsg);
                RaisePropertyChanged(() => FrameToSend);
            }
        }

        public int PortNum { get; set; }
        public int LocalPortNum { get; set; }

        public bool CarriageReturnTrue { get; set; }

        public bool LineFeedTrue { get; set; }

        public string FrameToSend => $"{this.messageSent}";




        // Auto SuggestBox

        private void GetIpSuggestionsFromDb()
        {
            stopwatch.Start();
            SQLiteCRUD sql = new SQLiteCRUD(ConnectionStringManager.GetConnectionString(ConnectionStringManager.DataBases.Network));
            int numOfSuggestions = 20;

            Serilog.Log.Logger.Information("Getting IP Address Suggestions {numOfSuggestions}", numOfSuggestions);

            var rows = sql.GetUdpUsedIPAddresses(numOfSuggestions);
            IpList = rows;

            stopwatch.Stop();
            string timeToFetchFromDB = $" DB query time: {stopwatch.ElapsedMilliseconds} ms";
            DataBaseQueryTime = timeToFetchFromDB;
            RaisePropertyChanged(() => IpList);
            RaisePropertyChanged(() => DataBaseQueryTime);
        }

        private void IpSuggestionBoxChanged()
        {

        }


        public IList<string> IpList { get; set; }

        public string DataBaseQueryTime { get; set; }

        public IMvxCommand IPBoxTextChangeCommand { get; set; }




    }
}
