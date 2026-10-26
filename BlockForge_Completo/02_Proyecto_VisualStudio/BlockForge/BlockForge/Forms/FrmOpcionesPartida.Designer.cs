namespace BlockForge.Forms
{
    partial class FrmOpcionesPartida
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
            this.lblTitulo = new System.Windows.Forms.Label();
            this.lblDescripcion = new System.Windows.Forms.Label();
            this.rdoLocal = new System.Windows.Forms.RadioButton();
            this.lblLocal = new System.Windows.Forms.Label();
            this.rdoTunel = new System.Windows.Forms.RadioButton();
            this.lblTunel = new System.Windows.Forms.Label();
            this.rdoInternet = new System.Windows.Forms.RadioButton();
            this.lblInternet = new System.Windows.Forms.Label();
            this.lblPuerto = new System.Windows.Forms.Label();
            this.nudPuerto = new System.Windows.Forms.NumericUpDown();
            this.lblMaximo = new System.Windows.Forms.Label();
            this.nudMaximo = new System.Windows.Forms.NumericUpDown();
            this.lblContrasena = new System.Windows.Forms.Label();
            this.txtContrasena = new System.Windows.Forms.TextBox();
            this.lblNotaContrasena = new System.Windows.Forms.Label();
            this.chkPvp = new System.Windows.Forms.CheckBox();
            this.btnCancelar = new System.Windows.Forms.Button();
            this.btnAbrir = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.nudPuerto)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMaximo)).BeginInit();
            this.SuspendLayout();
            //
            // lblTitulo
            //
            this.lblTitulo.AutoSize = true;
            this.lblTitulo.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblTitulo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(47)))), ((int)(((byte)(107)))), ((int)(((byte)(58)))));
            this.lblTitulo.Location = new System.Drawing.Point(22, 18);
            this.lblTitulo.Name = "lblTitulo";
            this.lblTitulo.Text = "Abrir partida";
            //
            // lblDescripcion
            //
            this.lblDescripcion.AutoSize = true;
            this.lblDescripcion.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(95)))), ((int)(((byte)(99)))), ((int)(((byte)(104)))));
            this.lblDescripcion.Location = new System.Drawing.Point(25, 56);
            this.lblDescripcion.Name = "lblDescripcion";
            this.lblDescripcion.Text = "Elige cómo podrán entrar los demás jugadores.";
            //
            // rdoLocal
            //
            this.rdoLocal.AutoSize = true;
            this.rdoLocal.Checked = true;
            this.rdoLocal.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.rdoLocal.Location = new System.Drawing.Point(28, 88);
            this.rdoLocal.Name = "rdoLocal";
            this.rdoLocal.TabIndex = 0;
            this.rdoLocal.TabStop = true;
            this.rdoLocal.Text = "Red local";
            //
            // lblLocal
            //
            this.lblLocal.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblLocal.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(95)))), ((int)(((byte)(99)))), ((int)(((byte)(104)))));
            this.lblLocal.Location = new System.Drawing.Point(46, 112);
            this.lblLocal.Name = "lblLocal";
            this.lblLocal.Size = new System.Drawing.Size(420, 20);
            this.lblLocal.Text = "Equipos conectados al mismo Wi-Fi o router. La partida aparece sola en su lista.";
            //
            // rdoTunel
            //
            this.rdoTunel.AutoSize = true;
            this.rdoTunel.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.rdoTunel.Location = new System.Drawing.Point(28, 140);
            this.rdoTunel.Name = "rdoTunel";
            this.rdoTunel.TabIndex = 1;
            this.rdoTunel.Text = "Internet fácil (túnel de Cloudflare) — recomendado";
            //
            // lblTunel
            //
            this.lblTunel.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblTunel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(95)))), ((int)(((byte)(99)))), ((int)(((byte)(104)))));
            this.lblTunel.Location = new System.Drawing.Point(46, 164);
            this.lblTunel.Name = "lblTunel";
            this.lblTunel.Size = new System.Drawing.Size(420, 34);
            this.lblTunel.Text = "Sin tocar el router ni crear cuentas: se obtiene una dirección https://...trycloudflare.com para compartir. Funciona aunque tu proveedor use CGNAT.";
            //
            // rdoInternet
            //
            this.rdoInternet.AutoSize = true;
            this.rdoInternet.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.rdoInternet.Location = new System.Drawing.Point(28, 202);
            this.rdoInternet.Name = "rdoInternet";
            this.rdoInternet.TabIndex = 1;
            this.rdoInternet.Text = "Internet abriendo un puerto (avanzado)";
            //
            // lblInternet
            //
            this.lblInternet.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblInternet.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(95)))), ((int)(((byte)(99)))), ((int)(((byte)(104)))));
            this.lblInternet.Location = new System.Drawing.Point(46, 226);
            this.lblInternet.Name = "lblInternet";
            this.lblInternet.Size = new System.Drawing.Size(420, 50);
            this.lblInternet.Text = "Amigos en cualquier lugar. Se abre el puerto del router automáticamente (UPnP) y se muestra tu IP pública para que la compartas. Usa una contraseña.";
            //
            // lblPuerto
            //
            this.lblPuerto.AutoSize = true;
            this.lblPuerto.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblPuerto.Location = new System.Drawing.Point(25, 282);
            this.lblPuerto.Name = "lblPuerto";
            this.lblPuerto.Text = "Puerto TCP";
            //
            // nudPuerto
            //
            this.nudPuerto.Location = new System.Drawing.Point(26, 305);
            this.nudPuerto.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.nudPuerto.Minimum = new decimal(new int[] {
            1024,
            0,
            0,
            0});
            this.nudPuerto.Name = "nudPuerto";
            this.nudPuerto.Size = new System.Drawing.Size(205, 25);
            this.nudPuerto.TabIndex = 2;
            this.nudPuerto.Value = new decimal(new int[] {
            25570,
            0,
            0,
            0});
            //
            // lblMaximo
            //
            this.lblMaximo.AutoSize = true;
            this.lblMaximo.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblMaximo.Location = new System.Drawing.Point(248, 282);
            this.lblMaximo.Name = "lblMaximo";
            this.lblMaximo.Text = "Máximo de jugadores";
            //
            // nudMaximo
            //
            this.nudMaximo.Location = new System.Drawing.Point(249, 305);
            this.nudMaximo.Maximum = new decimal(new int[] {
            32,
            0,
            0,
            0});
            this.nudMaximo.Minimum = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.nudMaximo.Name = "nudMaximo";
            this.nudMaximo.Size = new System.Drawing.Size(205, 25);
            this.nudMaximo.TabIndex = 3;
            this.nudMaximo.Value = new decimal(new int[] {
            8,
            0,
            0,
            0});
            //
            // lblContrasena
            //
            this.lblContrasena.AutoSize = true;
            this.lblContrasena.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblContrasena.Location = new System.Drawing.Point(25, 344);
            this.lblContrasena.Name = "lblContrasena";
            this.lblContrasena.Text = "Contraseña de la partida (opcional)";
            //
            // txtContrasena
            //
            this.txtContrasena.Location = new System.Drawing.Point(26, 367);
            this.txtContrasena.MaxLength = 40;
            this.txtContrasena.Name = "txtContrasena";
            this.txtContrasena.Size = new System.Drawing.Size(428, 25);
            this.txtContrasena.TabIndex = 4;
            //
            // lblNotaContrasena
            //
            this.lblNotaContrasena.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblNotaContrasena.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(95)))), ((int)(((byte)(99)))), ((int)(((byte)(104)))));
            this.lblNotaContrasena.Location = new System.Drawing.Point(26, 396);
            this.lblNotaContrasena.Name = "lblNotaContrasena";
            this.lblNotaContrasena.Size = new System.Drawing.Size(428, 36);
            this.lblNotaContrasena.Text = "Compártela solo con tus amigos. En la partida puedes escribir /expulsar nombre en el chat.";
            //
            // chkPvp
            //
            this.chkPvp.AutoSize = true;
            this.chkPvp.Checked = true;
            this.chkPvp.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkPvp.Location = new System.Drawing.Point(28, 436);
            this.chkPvp.Name = "chkPvp";
            this.chkPvp.TabIndex = 5;
            this.chkPvp.Text = "PvP: los jugadores pueden atacarse (se cambia con /pvp)";
            //
            // btnCancelar
            //
            this.btnCancelar.BackColor = System.Drawing.Color.White;
            this.btnCancelar.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCancelar.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(158)))), ((int)(((byte)(158)))), ((int)(((byte)(158)))));
            this.btnCancelar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCancelar.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnCancelar.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(33)))), ((int)(((byte)(33)))));
            this.btnCancelar.Location = new System.Drawing.Point(222, 478);
            this.btnCancelar.Name = "btnCancelar";
            this.btnCancelar.Size = new System.Drawing.Size(112, 36);
            this.btnCancelar.TabIndex = 6;
            this.btnCancelar.Text = "Cancelar";
            this.btnCancelar.UseVisualStyleBackColor = false;
            this.btnCancelar.Click += new System.EventHandler(this.btnCancelar_Click);
            //
            // btnAbrir
            //
            this.btnAbrir.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(47)))), ((int)(((byte)(107)))), ((int)(((byte)(58)))));
            this.btnAbrir.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnAbrir.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(36)))), ((int)(((byte)(84)))), ((int)(((byte)(45)))));
            this.btnAbrir.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAbrir.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnAbrir.ForeColor = System.Drawing.Color.White;
            this.btnAbrir.Location = new System.Drawing.Point(342, 478);
            this.btnAbrir.Name = "btnAbrir";
            this.btnAbrir.Size = new System.Drawing.Size(112, 36);
            this.btnAbrir.TabIndex = 5;
            this.btnAbrir.Text = "Abrir partida";
            this.btnAbrir.UseVisualStyleBackColor = false;
            this.btnAbrir.Click += new System.EventHandler(this.btnAbrir_Click);
            //
            // FrmOpcionesPartida
            //
            this.AcceptButton = this.btnAbrir;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(241)))), ((int)(((byte)(234)))));
            this.CancelButton = this.btnCancelar;
            this.ClientSize = new System.Drawing.Size(480, 534);
            this.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(33)))), ((int)(((byte)(33)))));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "BlockForge — Abrir partida";
            this.Controls.Add(this.lblTitulo);
            this.Controls.Add(this.lblDescripcion);
            this.Controls.Add(this.rdoLocal);
            this.Controls.Add(this.lblLocal);
            this.Controls.Add(this.rdoTunel);
            this.Controls.Add(this.lblTunel);
            this.Controls.Add(this.rdoInternet);
            this.Controls.Add(this.lblInternet);
            this.Controls.Add(this.lblPuerto);
            this.Controls.Add(this.nudPuerto);
            this.Controls.Add(this.lblMaximo);
            this.Controls.Add(this.nudMaximo);
            this.Controls.Add(this.lblContrasena);
            this.Controls.Add(this.txtContrasena);
            this.Controls.Add(this.lblNotaContrasena);
            this.Controls.Add(this.chkPvp);
            this.Controls.Add(this.btnCancelar);
            this.Controls.Add(this.btnAbrir);
            this.Name = "FrmOpcionesPartida";
            ((System.ComponentModel.ISupportInitialize)(this.nudPuerto)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMaximo)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblTitulo;
        private System.Windows.Forms.Label lblDescripcion;
        private System.Windows.Forms.RadioButton rdoLocal;
        private System.Windows.Forms.Label lblLocal;
        private System.Windows.Forms.RadioButton rdoTunel;
        private System.Windows.Forms.Label lblTunel;
        private System.Windows.Forms.RadioButton rdoInternet;
        private System.Windows.Forms.Label lblInternet;
        private System.Windows.Forms.Label lblPuerto;
        private System.Windows.Forms.NumericUpDown nudPuerto;
        private System.Windows.Forms.Label lblMaximo;
        private System.Windows.Forms.NumericUpDown nudMaximo;
        private System.Windows.Forms.Label lblContrasena;
        private System.Windows.Forms.TextBox txtContrasena;
        private System.Windows.Forms.Label lblNotaContrasena;
        private System.Windows.Forms.CheckBox chkPvp;
        private System.Windows.Forms.Button btnCancelar;
        private System.Windows.Forms.Button btnAbrir;
    }
}
