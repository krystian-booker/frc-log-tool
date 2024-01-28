namespace FRC_Log_Tool
{
    partial class FRCLogTool
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            lblStatus = new Label();
            contextMenuStrip1 = new ContextMenuStrip(components);
            progressBarImport = new ProgressBar();
            btnImport = new Button();
            rtbLog = new RichTextBox();
            lblRobotStatus = new Label();
            SuspendLayout();
            // 
            // lblStatus
            // 
            lblStatus.AutoSize = true;
            lblStatus.Font = new Font("Segoe UI", 16F);
            lblStatus.Location = new Point(12, 9);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(75, 30);
            lblStatus.TabIndex = 0;
            lblStatus.Text = "Status:";
            // 
            // contextMenuStrip1
            // 
            contextMenuStrip1.Name = "contextMenuStrip1";
            contextMenuStrip1.Size = new Size(61, 4);
            // 
            // progressBarImport
            // 
            progressBarImport.Location = new Point(12, 96);
            progressBarImport.Name = "progressBarImport";
            progressBarImport.Size = new Size(476, 46);
            progressBarImport.TabIndex = 2;
            // 
            // btnImport
            // 
            btnImport.Enabled = false;
            btnImport.Location = new Point(12, 42);
            btnImport.Name = "btnImport";
            btnImport.Size = new Size(476, 48);
            btnImport.TabIndex = 3;
            btnImport.Text = "Import";
            btnImport.UseVisualStyleBackColor = true;
            btnImport.Click += btnImport_Click;
            // 
            // rtbLog
            // 
            rtbLog.Location = new Point(12, 148);
            rtbLog.Name = "rtbLog";
            rtbLog.ReadOnly = true;
            rtbLog.Size = new Size(476, 251);
            rtbLog.TabIndex = 4;
            rtbLog.Text = "";
            // 
            // lblRobotStatus
            // 
            lblRobotStatus.AutoSize = true;
            lblRobotStatus.Font = new Font("Segoe UI", 16F);
            lblRobotStatus.Location = new Point(86, 9);
            lblRobotStatus.Name = "lblRobotStatus";
            lblRobotStatus.Size = new Size(0, 30);
            lblRobotStatus.TabIndex = 5;
            // 
            // FRCLogTool
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(500, 411);
            Controls.Add(lblRobotStatus);
            Controls.Add(rtbLog);
            Controls.Add(btnImport);
            Controls.Add(progressBarImport);
            Controls.Add(lblStatus);
            Name = "FRCLogTool";
            Text = "FRC Log Tool";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblStatus;
        private ContextMenuStrip contextMenuStrip1;
        private ProgressBar progressBarImport;
        private Button btnImport;
        private RichTextBox rtbLog;
        private Label lblRobotStatus;
    }
}
