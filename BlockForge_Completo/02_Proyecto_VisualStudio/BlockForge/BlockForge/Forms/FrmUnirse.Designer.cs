namespace BlockForge.Forms
{
    partial class FrmUnirse
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
            this.components = new System.ComponentModel.Container();
            this.lblTitulo = new System.Windows.Forms.Label();
            this.lblDescripcion = new System.Windows.Forms.Label();
            this.dgvPartidas = new System.Windows.Forms.DataGridView();
            this.lblDireccion = new System.Windows.Forms.Label();
            this.txtDireccion = new System.Windows.Forms.TextBox();
            this.lblClave = new System.Windows.Forms.Label();
            this.txtClave = new System.Windows.Forms.TextBox();
            this.lblJugador = new System.Windows.Forms.Label();
            this.lblError = new System.Windows.Forms.Label();
            this.btnCancelar = new System.Windows.Forms.Button();
            this.btnUnirse = new System.Windows.Forms.Button();
            this.tmrBuscar = new System.Windows.Forms.Timer(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.dgvPartidas)).BeginInit();
            this.SuspendLayout();
            //
            // lblTitulo
            //
            this.lblTitulo.AutoSize = true;
            this.lblTitulo.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblTitulo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(47)))), ((int)(((byte)(107)))), ((int)(((byte)(58)))));
            this.lblTitulo.Location = new System.Drawing.Point(22, 18);
            this.lblTitulo.Name = "lblTitulo";
            this.lblTitulo.Text = "Unirse a partida";
            //
            // lblDescripcion
            //
            this.lblDescripcion.AutoSize = true;
            this.lblDescripcion.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(95)))), ((int)(((byte)(99)))), ((int)(((byte)(104)))));
            this.lblDescripcion.Location = new System.Drawing.Point(25, 56);
            this.lblDescripcion.Name = "lblDescripcion";
            this.lblDescripcion.Text = "Partidas en tu red local (se actualiza sola). Para internet, pega la invitación abajo:";
            //
            // dgvPartidas
            //
            this.dgvPartidas.Location = new System.Drawing.Point(26, 82);
            this.dgvPartidas.Name = "dgvPartidas";
            this.dgvPartidas.Size = new System.Drawing.Size(508, 190);
            this.dgvPartidas.TabIndex = 0;
            this.dgvPartidas.SelectionChanged += new System.EventHandler(this.dgvPartidas_SelectionChanged);
            this.dgvPartidas.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvPartidas_CellDoubleClick);
            //
            // lblDireccion
            //
            this.lblDireccion.AutoSize = true;
            this.lblDireccion.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblDireccion.Location = new System.Drawing.Point(25, 288);
            this.lblDireccion.Name = "lblDireccion";
            this.lblDireccion.Text = "Dirección o invitación (IP:puerto)";
            //
            // txtDireccion
            //
            this.txtDireccion.Location = new System.Drawing.Point(26, 311);
            this.txtDireccion.MaxLength = 100;
            this.txtDireccion.Name = "txtDireccion";
            this.txtDireccion.Size = new System.Drawing.Size(330, 25);
            this.txtDireccion.TabIndex = 1;
            //
            // lblClave
            //
            this.lblClave.AutoSize = true;
            this.lblClave.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblClave.Location = new System.Drawing.Point(371, 288);
            this.lblClave.Name = "lblClave";
            this.lblClave.Text = "Contraseña";
            //
            // txtClave
            //
            this.txtClave.Location = new System.Drawing.Point(372, 311);
            this.txtClave.MaxLength = 40;
            this.txtClave.Name = "txtClave";
            this.txtClave.Size = new System.Drawing.Size(162, 25);
            this.txtClave.TabIndex = 2;
            this.txtClave.UseSystemPasswordChar = true;
            //
            // lblJugador
            //
            this.lblJugador.AutoSize = true;
            this.lblJugador.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblJugador.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(95)))), ((int)(((byte)(99)))), ((int)(((byte)(104)))));
            this.lblJugador.Location = new System.Drawing.Point(25, 342);
            this.lblJugador.Name = "lblJugador";
            this.lblJugador.Text = "Entrarás como:";
            //
            // lblError
            //
            this.lblError.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblError.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(198)))), ((int)(((byte)(40)))), ((int)(((byte)(40)))));
            this.lblError.Location = new System.Drawing.Point(26, 362);
            this.lblError.Name = "lblError";
            this.lblError.Size = new System.Drawing.Size(508, 40);
            this.lblError.Text = "";
            //
            // btnCancelar
            //
            this.btnCancelar.BackColor = System.Drawing.Color.White;
            this.btnCancelar.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCancelar.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(158)))), ((int)(((byte)(158)))), ((int)(((byte)(158)))));
            this.btnCancelar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCancelar.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnCancelar.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(33)))), ((int)(((byte)(33)))));
            this.btnCancelar.Location = new System.Drawing.Point(302, 410);
            this.btnCancelar.Name = "btnCancelar";
            this.btnCancelar.Size = new System.Drawing.Size(112, 36);
            this.btnCancelar.TabIndex = 4;
            this.btnCancelar.Text = "Cancelar";
            this.btnCancelar.UseVisualStyleBackColor = false;
            this.btnCancelar.Click += new System.EventHandler(this.btnCancelar_Click);
            //
            // btnUnirse
            //
            this.btnUnirse.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(47)))), ((int)(((byte)(107)))), ((int)(((byte)(58)))));
            this.btnUnirse.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnUnirse.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(36)))), ((int)(((byte)(84)))), ((int)(((byte)(45)))));
            this.btnUnirse.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnUnirse.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnUnirse.ForeColor = System.Drawing.Color.White;
            this.btnUnirse.Location = new System.Drawing.Point(422, 410);
            this.btnUnirse.Name = "btnUnirse";
            this.btnUnirse.Size = new System.Drawing.Size(112, 36);
            this.btnUnirse.TabIndex = 3;
            this.btnUnirse.Text = "Unirse";
            this.btnUnirse.UseVisualStyleBackColor = false;
            this.btnUnirse.Click += new System.EventHandler(this.btnUnirse_Click);
            //
            // tmrBuscar
            //
            this.tmrBuscar.Interval = 1000;
            this.tmrBuscar.Tick += new System.EventHandler(this.tmrBuscar_Tick);
            //
            // FrmUnirse
            //
            this.AcceptButton = this.btnUnirse;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(241)))), ((int)(((byte)(234)))));
            this.CancelButton = this.btnCancelar;
            this.ClientSize = new System.Drawing.Size(560, 468);
            this.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(33)))), ((int)(((byte)(33)))));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "BlockForge — Unirse a partida";
            this.Controls.Add(this.lblTitulo);
            this.Controls.Add(this.lblDescripcion);
            this.Controls.Add(this.dgvPartidas);
            this.Controls.Add(this.lblDireccion);
            this.Controls.Add(this.txtDireccion);
            this.Controls.Add(this.lblClave);
            this.Controls.Add(this.txtClave);
            this.Controls.Add(this.lblJugador);
            this.Controls.Add(this.lblError);
            this.Controls.Add(this.btnCancelar);
            this.Controls.Add(this.btnUnirse);
            this.Name = "FrmUnirse";
            this.Load += new System.EventHandler(this.FrmUnirse_Load);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FrmUnirse_FormClosed);
            ((System.ComponentModel.ISupportInitialize)(this.dgvPartidas)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblTitulo;
        private System.Windows.Forms.Label lblDescripcion;
        private System.Windows.Forms.DataGridView dgvPartidas;
        private System.Windows.Forms.Label lblDireccion;
        private System.Windows.Forms.TextBox txtDireccion;
        private System.Windows.Forms.Label lblClave;
        private System.Windows.Forms.TextBox txtClave;
        private System.Windows.Forms.Label lblJugador;
        private System.Windows.Forms.Label lblError;
        private System.Windows.Forms.Button btnCancelar;
        private System.Windows.Forms.Button btnUnirse;
        private System.Windows.Forms.Timer tmrBuscar;
    }
}
