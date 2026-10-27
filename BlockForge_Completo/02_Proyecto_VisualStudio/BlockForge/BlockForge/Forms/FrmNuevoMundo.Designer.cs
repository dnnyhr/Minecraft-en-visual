namespace BlockForge.Forms
{
    partial class FrmNuevoMundo
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
            this.lblNombre = new System.Windows.Forms.Label();
            this.txtNombre = new System.Windows.Forms.TextBox();
            this.lblErrorNombre = new System.Windows.Forms.Label();
            this.lblAncho = new System.Windows.Forms.Label();
            this.nudAncho = new System.Windows.Forms.NumericUpDown();
            this.lblAlto = new System.Windows.Forms.Label();
            this.nudAlto = new System.Windows.Forms.NumericUpDown();
            this.lblLargo = new System.Windows.Forms.Label();
            this.nudLargo = new System.Windows.Forms.NumericUpDown();
            this.lblRangos = new System.Windows.Forms.Label();
            this.chkLagos = new System.Windows.Forms.CheckBox();
            this.chkCasa = new System.Windows.Forms.CheckBox();
            this.btnCancelar = new System.Windows.Forms.Button();
            this.btnCrear = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.nudAncho)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudAlto)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudLargo)).BeginInit();
            this.SuspendLayout();
            // 
            // lblTitulo
            // 
            this.lblTitulo.AutoSize = true;
            this.lblTitulo.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblTitulo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(47)))), ((int)(((byte)(107)))), ((int)(((byte)(58)))));
            this.lblTitulo.Location = new System.Drawing.Point(22, 18);
            this.lblTitulo.Name = "lblTitulo";
            this.lblTitulo.Text = "Nuevo mundo";
            // 
            // lblDescripcion
            // 
            this.lblDescripcion.AutoSize = true;
            this.lblDescripcion.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblDescripcion.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(95)))), ((int)(((byte)(99)))), ((int)(((byte)(104)))));
            this.lblDescripcion.Location = new System.Drawing.Point(25, 56);
            this.lblDescripcion.Name = "lblDescripcion";
            this.lblDescripcion.Text = "Define el nombre y las dimensiones del terreno 3D.";
            // 
            // lblNombre
            // 
            this.lblNombre.AutoSize = true;
            this.lblNombre.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblNombre.Location = new System.Drawing.Point(25, 94);
            this.lblNombre.Name = "lblNombre";
            this.lblNombre.Text = "Nombre del mundo";
            // 
            // txtNombre
            // 
            this.txtNombre.Location = new System.Drawing.Point(26, 117);
            this.txtNombre.MaxLength = 40;
            this.txtNombre.Name = "txtNombre";
            this.txtNombre.Size = new System.Drawing.Size(428, 25);
            this.txtNombre.TabIndex = 0;
            this.txtNombre.TextChanged += new System.EventHandler(this.txtNombre_TextChanged);
            // 
            // lblErrorNombre
            // 
            this.lblErrorNombre.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblErrorNombre.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(198)))), ((int)(((byte)(40)))), ((int)(((byte)(40)))));
            this.lblErrorNombre.Location = new System.Drawing.Point(26, 146);
            this.lblErrorNombre.Name = "lblErrorNombre";
            this.lblErrorNombre.Size = new System.Drawing.Size(428, 18);
            this.lblErrorNombre.Text = "";
            // 
            // lblAncho
            // 
            this.lblAncho.AutoSize = true;
            this.lblAncho.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblAncho.Location = new System.Drawing.Point(25, 174);
            this.lblAncho.Name = "lblAncho";
            this.lblAncho.Text = "Ancho (bloques)";
            // 
            // nudAncho
            // 
            this.nudAncho.Increment = new decimal(new int[] {
            16,
            0,
            0,
            0});
            this.nudAncho.Location = new System.Drawing.Point(26, 197);
            this.nudAncho.Maximum = new decimal(new int[] {
            256,
            0,
            0,
            0});
            this.nudAncho.Minimum = new decimal(new int[] {
            32,
            0,
            0,
            0});
            this.nudAncho.Name = "nudAncho";
            this.nudAncho.Size = new System.Drawing.Size(130, 25);
            this.nudAncho.TabIndex = 1;
            this.nudAncho.Value = new decimal(new int[] {
            128,
            0,
            0,
            0});
            //
            // lblLargo
            //
            this.lblLargo.AutoSize = true;
            this.lblLargo.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblLargo.Location = new System.Drawing.Point(173, 174);
            this.lblLargo.Name = "lblLargo";
            this.lblLargo.Text = "Largo (bloques)";
            //
            // nudLargo
            //
            this.nudLargo.Increment = new decimal(new int[] {
            16,
            0,
            0,
            0});
            this.nudLargo.Location = new System.Drawing.Point(175, 197);
            this.nudLargo.Maximum = new decimal(new int[] {
            256,
            0,
            0,
            0});
            this.nudLargo.Minimum = new decimal(new int[] {
            32,
            0,
            0,
            0});
            this.nudLargo.Name = "nudLargo";
            this.nudLargo.Size = new System.Drawing.Size(130, 25);
            this.nudLargo.TabIndex = 2;
            this.nudLargo.Value = new decimal(new int[] {
            128,
            0,
            0,
            0});
            //
            // lblAlto
            //
            this.lblAlto.AutoSize = true;
            this.lblAlto.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblAlto.Location = new System.Drawing.Point(322, 174);
            this.lblAlto.Name = "lblAlto";
            this.lblAlto.Text = "Altura (bloques)";
            //
            // nudAlto
            //
            this.nudAlto.Increment = new decimal(new int[] {
            16,
            0,
            0,
            0});
            this.nudAlto.Location = new System.Drawing.Point(324, 197);
            this.nudAlto.Maximum = new decimal(new int[] {
            128,
            0,
            0,
            0});
            this.nudAlto.Minimum = new decimal(new int[] {
            32,
            0,
            0,
            0});
            this.nudAlto.Name = "nudAlto";
            this.nudAlto.Size = new System.Drawing.Size(130, 25);
            this.nudAlto.TabIndex = 3;
            this.nudAlto.Value = new decimal(new int[] {
            64,
            0,
            0,
            0});
            // 
            // lblRangos
            // 
            this.lblRangos.AutoSize = true;
            this.lblRangos.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblRangos.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(95)))), ((int)(((byte)(99)))), ((int)(((byte)(104)))));
            this.lblRangos.Location = new System.Drawing.Point(25, 229);
            this.lblRangos.Name = "lblRangos";
            this.lblRangos.Text = "Ancho y largo: 32 a 256 · Altura: 32 a 128 · Nivel del mar: mitad de la altura";
            // 
            // chkLagos
            // 
            this.chkLagos.AutoSize = true;
            this.chkLagos.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkLagos.Checked = true;
            this.chkLagos.Location = new System.Drawing.Point(26, 262);
            this.chkLagos.Name = "chkLagos";
            this.chkLagos.TabIndex = 4;
            this.chkLagos.Text = "Generar lagos de agua";
            //
            // chkCasa
            //
            this.chkCasa.AutoSize = true;
            this.chkCasa.Location = new System.Drawing.Point(26, 292);
            this.chkCasa.Name = "chkCasa";
            this.chkCasa.TabIndex = 5;
            this.chkCasa.Text = "Empezar con una casita con jardín y materiales";
            //
            // btnCancelar
            // 
            this.btnCancelar.BackColor = System.Drawing.Color.White;
            this.btnCancelar.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCancelar.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(158)))), ((int)(((byte)(158)))), ((int)(((byte)(158)))));
            this.btnCancelar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCancelar.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnCancelar.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(33)))), ((int)(((byte)(33)))));
            this.btnCancelar.Location = new System.Drawing.Point(222, 336);
            this.btnCancelar.Name = "btnCancelar";
            this.btnCancelar.Size = new System.Drawing.Size(112, 36);
            this.btnCancelar.TabIndex = 6;
            this.btnCancelar.Text = "Cancelar";
            this.btnCancelar.Click += new System.EventHandler(this.btnCancelar_Click);
            // 
            // btnCrear
            // 
            this.btnCrear.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(47)))), ((int)(((byte)(107)))), ((int)(((byte)(58)))));
            this.btnCrear.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCrear.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(36)))), ((int)(((byte)(84)))), ((int)(((byte)(45)))));
            this.btnCrear.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCrear.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnCrear.ForeColor = System.Drawing.Color.White;
            this.btnCrear.Location = new System.Drawing.Point(342, 336);
            this.btnCrear.Name = "btnCrear";
            this.btnCrear.Size = new System.Drawing.Size(112, 36);
            this.btnCrear.TabIndex = 5;
            this.btnCrear.Text = "Crear mundo";
            this.btnCrear.Click += new System.EventHandler(this.btnCrear_Click);
            // 
            // FrmNuevoMundo
            // 
            this.AcceptButton = this.btnCrear;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(241)))), ((int)(((byte)(234)))));
            this.CancelButton = this.btnCancelar;
            this.ClientSize = new System.Drawing.Size(480, 392);
            this.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(33)))), ((int)(((byte)(33)))));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "BlockForge — Nuevo mundo";
            this.Controls.Add(this.lblTitulo);
            this.Controls.Add(this.lblDescripcion);
            this.Controls.Add(this.lblNombre);
            this.Controls.Add(this.txtNombre);
            this.Controls.Add(this.lblErrorNombre);
            this.Controls.Add(this.lblAncho);
            this.Controls.Add(this.nudAncho);
            this.Controls.Add(this.lblAlto);
            this.Controls.Add(this.lblLargo);
            this.Controls.Add(this.nudLargo);
            this.Controls.Add(this.nudAlto);
            this.Controls.Add(this.lblRangos);
            this.Controls.Add(this.chkLagos);
            this.Controls.Add(this.chkCasa);
            this.Controls.Add(this.btnCancelar);
            this.Controls.Add(this.btnCrear);
            this.Name = "FrmNuevoMundo";
            this.Load += new System.EventHandler(this.FrmNuevoMundo_Load);
            ((System.ComponentModel.ISupportInitialize)(this.nudAncho)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudAlto)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudLargo)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblTitulo;
        private System.Windows.Forms.Label lblDescripcion;
        private System.Windows.Forms.Label lblNombre;
        private System.Windows.Forms.TextBox txtNombre;
        private System.Windows.Forms.Label lblErrorNombre;
        private System.Windows.Forms.Label lblAncho;
        private System.Windows.Forms.NumericUpDown nudAncho;
        private System.Windows.Forms.Label lblAlto;
        private System.Windows.Forms.NumericUpDown nudAlto;
        private System.Windows.Forms.Label lblLargo;
        private System.Windows.Forms.NumericUpDown nudLargo;
        private System.Windows.Forms.Label lblRangos;
        private System.Windows.Forms.CheckBox chkLagos;
        private System.Windows.Forms.CheckBox chkCasa;
        private System.Windows.Forms.Button btnCancelar;
        private System.Windows.Forms.Button btnCrear;
    }
}
