namespace BlockForge.Forms
{
    partial class FrmConfiguracion
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
            this.grpVisualizacion = new System.Windows.Forms.GroupBox();
            this.lblDistancia = new System.Windows.Forms.Label();
            this.trkDistancia = new System.Windows.Forms.TrackBar();
            this.lblValorDistancia = new System.Windows.Forms.Label();
            this.lblCampo = new System.Windows.Forms.Label();
            this.trkCampo = new System.Windows.Forms.TrackBar();
            this.lblValorCampo = new System.Windows.Forms.Label();
            this.lblSensibilidad = new System.Windows.Forms.Label();
            this.trkSensibilidad = new System.Windows.Forms.TrackBar();
            this.lblValorSensibilidad = new System.Windows.Forms.Label();
            this.grpMundo = new System.Windows.Forms.GroupBox();
            this.lblAncho = new System.Windows.Forms.Label();
            this.nudAncho = new System.Windows.Forms.NumericUpDown();
            this.lblLargo = new System.Windows.Forms.Label();
            this.nudLargo = new System.Windows.Forms.NumericUpDown();
            this.lblAlto = new System.Windows.Forms.Label();
            this.nudAlto = new System.Windows.Forms.NumericUpDown();
            this.lblModificacion = new System.Windows.Forms.Label();
            this.btnRestablecer = new System.Windows.Forms.Button();
            this.btnCancelar = new System.Windows.Forms.Button();
            this.btnGuardar = new System.Windows.Forms.Button();
            this.grpVisualizacion.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trkDistancia)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trkCampo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trkSensibilidad)).BeginInit();
            this.grpMundo.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudAncho)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudLargo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudAlto)).BeginInit();
            this.SuspendLayout();
            //
            // lblTitulo
            //
            this.lblTitulo.AutoSize = true;
            this.lblTitulo.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblTitulo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(47)))), ((int)(((byte)(107)))), ((int)(((byte)(58)))));
            this.lblTitulo.Location = new System.Drawing.Point(22, 18);
            this.lblTitulo.Name = "lblTitulo";
            this.lblTitulo.Text = "Configuración";
            //
            // lblDescripcion
            //
            this.lblDescripcion.AutoSize = true;
            this.lblDescripcion.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblDescripcion.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(95)))), ((int)(((byte)(99)))), ((int)(((byte)(104)))));
            this.lblDescripcion.Location = new System.Drawing.Point(25, 56);
            this.lblDescripcion.Name = "lblDescripcion";
            this.lblDescripcion.Text = "Opciones generales de la aplicación.";
            //
            // grpVisualizacion
            //
            this.grpVisualizacion.Controls.Add(this.lblDistancia);
            this.grpVisualizacion.Controls.Add(this.trkDistancia);
            this.grpVisualizacion.Controls.Add(this.lblValorDistancia);
            this.grpVisualizacion.Controls.Add(this.lblCampo);
            this.grpVisualizacion.Controls.Add(this.trkCampo);
            this.grpVisualizacion.Controls.Add(this.lblValorCampo);
            this.grpVisualizacion.Controls.Add(this.lblSensibilidad);
            this.grpVisualizacion.Controls.Add(this.trkSensibilidad);
            this.grpVisualizacion.Controls.Add(this.lblValorSensibilidad);
            this.grpVisualizacion.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.grpVisualizacion.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(139)))), ((int)(((byte)(94)))), ((int)(((byte)(52)))));
            this.grpVisualizacion.Location = new System.Drawing.Point(26, 92);
            this.grpVisualizacion.Name = "grpVisualizacion";
            this.grpVisualizacion.Size = new System.Drawing.Size(468, 236);
            this.grpVisualizacion.Text = "Visualización y controles";
            //
            // lblDistancia
            //
            this.lblDistancia.AutoSize = true;
            this.lblDistancia.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(33)))), ((int)(((byte)(33)))));
            this.lblDistancia.Location = new System.Drawing.Point(15, 28);
            this.lblDistancia.Name = "lblDistancia";
            this.lblDistancia.Text = "Distancia de visión";
            //
            // trkDistancia
            //
            this.trkDistancia.LargeChange = 32;
            this.trkDistancia.Location = new System.Drawing.Point(12, 48);
            this.trkDistancia.Maximum = 256;
            this.trkDistancia.Minimum = 32;
            this.trkDistancia.Name = "trkDistancia";
            this.trkDistancia.Size = new System.Drawing.Size(340, 45);
            this.trkDistancia.SmallChange = 16;
            this.trkDistancia.TickFrequency = 32;
            this.trkDistancia.Value = 128;
            this.trkDistancia.ValueChanged += new System.EventHandler(this.trk_ValueChanged);
            //
            // lblValorDistancia
            //
            this.lblValorDistancia.BackColor = System.Drawing.Color.White;
            this.lblValorDistancia.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblValorDistancia.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblValorDistancia.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(33)))), ((int)(((byte)(33)))));
            this.lblValorDistancia.Location = new System.Drawing.Point(358, 50);
            this.lblValorDistancia.Name = "lblValorDistancia";
            this.lblValorDistancia.Size = new System.Drawing.Size(96, 27);
            this.lblValorDistancia.Text = "128 bloques";
            this.lblValorDistancia.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // lblCampo
            //
            this.lblCampo.AutoSize = true;
            this.lblCampo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(33)))), ((int)(((byte)(33)))));
            this.lblCampo.Location = new System.Drawing.Point(15, 96);
            this.lblCampo.Name = "lblCampo";
            this.lblCampo.Text = "Campo de visión";
            //
            // trkCampo
            //
            this.trkCampo.LargeChange = 10;
            this.trkCampo.Location = new System.Drawing.Point(12, 116);
            this.trkCampo.Maximum = 110;
            this.trkCampo.Minimum = 50;
            this.trkCampo.Name = "trkCampo";
            this.trkCampo.Size = new System.Drawing.Size(340, 45);
            this.trkCampo.SmallChange = 5;
            this.trkCampo.TickFrequency = 10;
            this.trkCampo.Value = 70;
            this.trkCampo.ValueChanged += new System.EventHandler(this.trk_ValueChanged);
            //
            // lblValorCampo
            //
            this.lblValorCampo.BackColor = System.Drawing.Color.White;
            this.lblValorCampo.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblValorCampo.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblValorCampo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(33)))), ((int)(((byte)(33)))));
            this.lblValorCampo.Location = new System.Drawing.Point(358, 118);
            this.lblValorCampo.Name = "lblValorCampo";
            this.lblValorCampo.Size = new System.Drawing.Size(96, 27);
            this.lblValorCampo.Text = "70°";
            this.lblValorCampo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // lblSensibilidad
            //
            this.lblSensibilidad.AutoSize = true;
            this.lblSensibilidad.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(33)))), ((int)(((byte)(33)))));
            this.lblSensibilidad.Location = new System.Drawing.Point(15, 164);
            this.lblSensibilidad.Name = "lblSensibilidad";
            this.lblSensibilidad.Text = "Sensibilidad del mouse";
            //
            // trkSensibilidad
            //
            this.trkSensibilidad.LargeChange = 2;
            this.trkSensibilidad.Location = new System.Drawing.Point(12, 184);
            this.trkSensibilidad.Maximum = 10;
            this.trkSensibilidad.Minimum = 1;
            this.trkSensibilidad.Name = "trkSensibilidad";
            this.trkSensibilidad.Size = new System.Drawing.Size(340, 45);
            this.trkSensibilidad.Value = 5;
            this.trkSensibilidad.ValueChanged += new System.EventHandler(this.trk_ValueChanged);
            //
            // lblValorSensibilidad
            //
            this.lblValorSensibilidad.BackColor = System.Drawing.Color.White;
            this.lblValorSensibilidad.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblValorSensibilidad.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblValorSensibilidad.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(33)))), ((int)(((byte)(33)))));
            this.lblValorSensibilidad.Location = new System.Drawing.Point(358, 186);
            this.lblValorSensibilidad.Name = "lblValorSensibilidad";
            this.lblValorSensibilidad.Size = new System.Drawing.Size(96, 27);
            this.lblValorSensibilidad.Text = "5";
            this.lblValorSensibilidad.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // grpMundo
            //
            this.grpMundo.Controls.Add(this.lblAncho);
            this.grpMundo.Controls.Add(this.nudAncho);
            this.grpMundo.Controls.Add(this.lblLargo);
            this.grpMundo.Controls.Add(this.nudLargo);
            this.grpMundo.Controls.Add(this.lblAlto);
            this.grpMundo.Controls.Add(this.nudAlto);
            this.grpMundo.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.grpMundo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(139)))), ((int)(((byte)(94)))), ((int)(((byte)(52)))));
            this.grpMundo.Location = new System.Drawing.Point(26, 342);
            this.grpMundo.Name = "grpMundo";
            this.grpMundo.Size = new System.Drawing.Size(468, 96);
            this.grpMundo.Text = "Mundo predeterminado";
            //
            // lblAncho
            //
            this.lblAncho.AutoSize = true;
            this.lblAncho.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(33)))), ((int)(((byte)(33)))));
            this.lblAncho.Location = new System.Drawing.Point(15, 28);
            this.lblAncho.Name = "lblAncho";
            this.lblAncho.Text = "Ancho";
            //
            // nudAncho
            //
            this.nudAncho.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.nudAncho.Increment = new decimal(new int[] {
            16,
            0,
            0,
            0});
            this.nudAncho.Location = new System.Drawing.Point(16, 51);
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
            this.nudAncho.Value = new decimal(new int[] {
            128,
            0,
            0,
            0});
            //
            // lblLargo
            //
            this.lblLargo.AutoSize = true;
            this.lblLargo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(33)))), ((int)(((byte)(33)))));
            this.lblLargo.Location = new System.Drawing.Point(164, 28);
            this.lblLargo.Name = "lblLargo";
            this.lblLargo.Text = "Largo";
            //
            // nudLargo
            //
            this.nudLargo.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.nudLargo.Increment = new decimal(new int[] {
            16,
            0,
            0,
            0});
            this.nudLargo.Location = new System.Drawing.Point(165, 51);
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
            this.nudLargo.Value = new decimal(new int[] {
            128,
            0,
            0,
            0});
            //
            // lblAlto
            //
            this.lblAlto.AutoSize = true;
            this.lblAlto.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(33)))), ((int)(((byte)(33)))));
            this.lblAlto.Location = new System.Drawing.Point(313, 28);
            this.lblAlto.Name = "lblAlto";
            this.lblAlto.Text = "Altura";
            //
            // nudAlto
            //
            this.nudAlto.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.nudAlto.Increment = new decimal(new int[] {
            16,
            0,
            0,
            0});
            this.nudAlto.Location = new System.Drawing.Point(314, 51);
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
            this.nudAlto.Size = new System.Drawing.Size(136, 25);
            this.nudAlto.Value = new decimal(new int[] {
            64,
            0,
            0,
            0});
            //
            // lblModificacion
            //
            this.lblModificacion.AutoSize = true;
            this.lblModificacion.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblModificacion.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(95)))), ((int)(((byte)(99)))), ((int)(((byte)(104)))));
            this.lblModificacion.Location = new System.Drawing.Point(25, 452);
            this.lblModificacion.Name = "lblModificacion";
            this.lblModificacion.Text = "Última modificación:";
            //
            // btnRestablecer
            //
            this.btnRestablecer.BackColor = System.Drawing.Color.White;
            this.btnRestablecer.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnRestablecer.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(158)))), ((int)(((byte)(158)))), ((int)(((byte)(158)))));
            this.btnRestablecer.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRestablecer.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnRestablecer.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(33)))), ((int)(((byte)(33)))));
            this.btnRestablecer.Location = new System.Drawing.Point(26, 484);
            this.btnRestablecer.Name = "btnRestablecer";
            this.btnRestablecer.Size = new System.Drawing.Size(160, 36);
            this.btnRestablecer.Text = "Restablecer valores";
            this.btnRestablecer.Click += new System.EventHandler(this.btnRestablecer_Click);
            //
            // btnCancelar
            //
            this.btnCancelar.BackColor = System.Drawing.Color.White;
            this.btnCancelar.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCancelar.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(158)))), ((int)(((byte)(158)))), ((int)(((byte)(158)))));
            this.btnCancelar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCancelar.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnCancelar.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(33)))), ((int)(((byte)(33)))));
            this.btnCancelar.Location = new System.Drawing.Point(262, 484);
            this.btnCancelar.Name = "btnCancelar";
            this.btnCancelar.Size = new System.Drawing.Size(112, 36);
            this.btnCancelar.Text = "Cancelar";
            this.btnCancelar.Click += new System.EventHandler(this.btnCancelar_Click);
            //
            // btnGuardar
            //
            this.btnGuardar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(47)))), ((int)(((byte)(107)))), ((int)(((byte)(58)))));
            this.btnGuardar.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnGuardar.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(36)))), ((int)(((byte)(84)))), ((int)(((byte)(45)))));
            this.btnGuardar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnGuardar.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnGuardar.ForeColor = System.Drawing.Color.White;
            this.btnGuardar.Location = new System.Drawing.Point(382, 484);
            this.btnGuardar.Name = "btnGuardar";
            this.btnGuardar.Size = new System.Drawing.Size(112, 36);
            this.btnGuardar.Text = "Guardar";
            this.btnGuardar.Click += new System.EventHandler(this.btnGuardar_Click);
            //
            // FrmConfiguracion
            //
            this.AcceptButton = this.btnGuardar;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(241)))), ((int)(((byte)(234)))));
            this.CancelButton = this.btnCancelar;
            this.ClientSize = new System.Drawing.Size(520, 542);
            this.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(33)))), ((int)(((byte)(33)))));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "BlockForge — Configuración";
            this.Controls.Add(this.lblTitulo);
            this.Controls.Add(this.lblDescripcion);
            this.Controls.Add(this.grpVisualizacion);
            this.Controls.Add(this.grpMundo);
            this.Controls.Add(this.lblModificacion);
            this.Controls.Add(this.btnRestablecer);
            this.Controls.Add(this.btnCancelar);
            this.Controls.Add(this.btnGuardar);
            this.Name = "FrmConfiguracion";
            this.Load += new System.EventHandler(this.FrmConfiguracion_Load);
            this.grpVisualizacion.ResumeLayout(false);
            this.grpVisualizacion.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trkDistancia)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trkCampo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trkSensibilidad)).EndInit();
            this.grpMundo.ResumeLayout(false);
            this.grpMundo.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudAncho)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudLargo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudAlto)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblTitulo;
        private System.Windows.Forms.Label lblDescripcion;
        private System.Windows.Forms.GroupBox grpVisualizacion;
        private System.Windows.Forms.Label lblDistancia;
        private System.Windows.Forms.TrackBar trkDistancia;
        private System.Windows.Forms.Label lblValorDistancia;
        private System.Windows.Forms.Label lblCampo;
        private System.Windows.Forms.TrackBar trkCampo;
        private System.Windows.Forms.Label lblValorCampo;
        private System.Windows.Forms.Label lblSensibilidad;
        private System.Windows.Forms.TrackBar trkSensibilidad;
        private System.Windows.Forms.Label lblValorSensibilidad;
        private System.Windows.Forms.GroupBox grpMundo;
        private System.Windows.Forms.Label lblAncho;
        private System.Windows.Forms.NumericUpDown nudAncho;
        private System.Windows.Forms.Label lblLargo;
        private System.Windows.Forms.NumericUpDown nudLargo;
        private System.Windows.Forms.Label lblAlto;
        private System.Windows.Forms.NumericUpDown nudAlto;
        private System.Windows.Forms.Label lblModificacion;
        private System.Windows.Forms.Button btnRestablecer;
        private System.Windows.Forms.Button btnCancelar;
        private System.Windows.Forms.Button btnGuardar;
    }
}
