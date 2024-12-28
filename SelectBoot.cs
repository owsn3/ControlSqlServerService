using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Timers;
using System.Windows.Forms;
using System.Windows.Threading;
using Timer = System.Timers.Timer;


namespace SQLServer起動アプリケーション
{
    public partial class SelectBoot : Form
    {
        /// <summary>
        /// タイマー
        /// </summary>
        private Timer mServiceMonitorTimer;

        /// <summary>
        /// 監視する間隔の秒数
        /// </summary>
        private readonly int mTime = 2000;

        /// <summary>
        /// サービス
        /// </summary>
        public ServiceController Service { get; set; }

        /// <summary>
        /// Serviceの状態を変更するデリゲート
        /// </summary>
        public Action<string> DelControlService { get; set; }

        /// <summary>
        /// 管理者権限かチェックする
        /// </summary>
        public Action DelCheckAdmin { get; set; }


        public SelectBoot()
        {
            InitializeComponent();
        }

        private void btnStart_Click(object sender, EventArgs e) => DelControlService.Invoke("Start");

        private void btnStop_Click(object sender, EventArgs e) => DelControlService.Invoke("Stop");

        private void btnCancel_Click(object sender, EventArgs e) => DelControlService.Invoke("Cancel");

        private void btnContinue_Click(object sender, EventArgs e) => DelControlService.Invoke("Continue");

        /// <summary>
        /// Load
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SelectBoot_Load(object sender, EventArgs e)
        {
            DelCheckAdmin.Invoke();

            // タイマーを初期化
            mServiceMonitorTimer = new Timer(mTime); // 1秒ごとにチェック
            mServiceMonitorTimer.Elapsed += OnServiceMonitorTick;
            mServiceMonitorTimer.Start();

            
            // 初期状態を反映
            ChangeButton();
        }

        /// <summary>
        /// Form終了
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SelectBoot_FormClosing(object sender, FormClosingEventArgs e)
        {
            mServiceMonitorTimer?.Stop();
            mServiceMonitorTimer?.Dispose();
        }

        /// <summary>
        /// タイマーの間隔で発火します。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnServiceMonitorTick(object sender, ElapsedEventArgs e) => ChangeButton();

        private void ChangeButton()
        {
            try
            {
                // 最新の状態にする
                Service.Refresh();
                var status = Service.Status.ToString();
                // メインスレッドからコントロールにアクセスする
                this.Invoke(new Action(() => UpdateControl(status)));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// ボタン制御
        /// </summary>
        /// <param name="status">サービスの状態</param>
        private void UpdateControl(string status)
        {
            this.btnStart.Enabled = (status == "Stopped" );
            this.btnStop.Enabled = (status == "Running" || status == "Paused");
            this.btnCancel.Enabled = (status == "Running");
            this.btnContinue.Enabled = (status == "Paused");

            // 状態遷移中（Pending）ではすべて無効化
            if (status.Contains("Pending"))
            {
                this.btnStart.Enabled = false;
                this.btnStop.Enabled = false;
                this.btnCancel.Enabled = false;
                this.btnContinue.Enabled = false;
            }
        }
    }
}
