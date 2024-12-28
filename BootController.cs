using ControlzEx.Standard;
using System;
using System.Diagnostics;
using System.Security.Principal;
using System.ServiceProcess;
using System.Timers;
using System.Web.Management;
using System.Windows.Forms;


namespace SQLServer起動アプリケーション
{
    public class BootController
    {
        /// <summary>
        /// サービス名
        /// </summary>
        private readonly string ServiceName = "MSSQLSERVER";
        /// <summary>
        /// 端末名
        /// </summary>
        private readonly string DeviceName = ".";

        private ServiceController mService;

        /// <summary>
        /// Form
        /// </summary>
        private SelectBoot frmSelectBoot;

        public BootController()
        {
            
            frmSelectBoot = new SelectBoot();
            ControlLogic();
            frmSelectBoot.ShowDialog();
        }

        /// <summary>
        /// 制御に必要な設定がされているか確認します。
        /// </summary>
        private void ControlLogic()
        {
            //ServiceControllerオブジェクトの作成
            mService = new ServiceController(ServiceName, DeviceName);
            frmSelectBoot.Service = mService;
            // デリゲートをセットします。
            SetDelegate();
            // サービスの状態を監視し、ボタンの活性状態を切り替えます。
        }

        private void MoniterTick(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// デリゲートをセットします。
        /// </summary>
        private void SetDelegate()
        {
            frmSelectBoot.DelControlService = ControlService;
            frmSelectBoot.DelCheckAdmin = CheckRunAdmin;
        }


        #region 管理者実行チェック
        /// <summary>
        /// 管理権限で実行しているか確認します。
        /// </summary>
        private void CheckRunAdmin()
        {
            // 管理者権限で実行されているかを確認
            if (!IsRunAsAdmin())
            {
                // 自身を管理者権限で再起動
                ElevateToAdministrator();
                return;
            }
        }

        /// <summary>
        /// 自身を管理者権限で再起動します。
        /// </summary>
        private void ElevateToAdministrator()
        {
            ProcessStartInfo processInfo = new ProcessStartInfo
            {
                FileName = Process.GetCurrentProcess().MainModule.FileName,
                UseShellExecute = true,
                Verb = "runas" // 管理者権限で実行するための設定
            };

            try
            {
                Process.Start(processInfo);
                frmSelectBoot.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("管理者権限で起動できませんでした" + ex.ToString());
            }
        }

        /// <summary>
        /// 管理者権限で実行されているかを確認します。
        /// </summary>
        /// <returns>true:管理者 false:管理者以外</returns>
        private bool IsRunAsAdmin()
        {
            // 現在のユーザを取得
            using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
            {
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
        }

        #endregion

        #region サービスの状態変更
        /// <summary>
        /// サービス制御メソッド
        /// </summary>
        /// <param name="key"></param>
        private void ControlService(string key) => ChangeService(key);

        /// <summary>
        /// 押したボタンに応じてサービスの状態を変更します。
        /// </summary>
        /// <param name="key"></param>
        private void ChangeService(string key)
        {
            try
            {
                switch (key)
                {
                    case "Start":
                        if (mService.Status == ServiceControllerStatus.Running) { return; }
                        mService.Start(); 
                        break;
                    case "Stop":
                        if (mService.Status == ServiceControllerStatus.Stopped) { return; }
                        mService.Stop();  
                        break;
                    case "Cancel":
                        if (!mService.CanPauseAndContinue ||mService.Status == ServiceControllerStatus.Paused) { return; }
                        mService.Pause();  
                        break;
                    case "Continue":
                        if (!mService.CanPauseAndContinue || mService.Status == ServiceControllerStatus.ContinuePending) { return; }
                        mService.Continue();  
                        break;
                    default:
                        break;
                }

            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }
        #endregion
    }
}