    using System;
    using System.Windows.Forms;
 
    public class MaskedTextBoxEditingControl : MaskedTextBox, IDataGridViewEditingControl
    {
        DataGridView dataGridView;
        
        bool valueChanged;
 
        int rowIndex;
 
        public void ApplyCellStyleToEditingControl(DataGridViewCellStyle dataGridViewCellStyle)
        {
            this.Font = dataGridViewCellStyle.Font;
            this.BackColor = dataGridViewCellStyle.BackColor;
            this.ForeColor = dataGridViewCellStyle.ForeColor;
            this.Tag = dataGridViewCellStyle.Tag;
        }
 
        public DataGridView EditingControlDataGridView
        {
            get
            {
                return this.dataGridView;
            }
            set
            {
                this.dataGridView = value;
            }
        }
 
        public object EditingControlFormattedValue
        {
            get
            {
                return this.Text;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                this.Text = value.ToString();
            }
        }
 
        public int EditingControlRowIndex
        {
            get
            {
                return rowIndex;
            }
            set
            {
                rowIndex = value;
            }
        }
 
        public bool EditingControlValueChanged
        {
            get
            {
                return valueChanged;
            }
            set
            {
                valueChanged = value;
            }
        }
 
        public bool EditingControlWantsInputKey(Keys keyData, bool dataGridViewWantsInputKey)
        {
            return false;
        }
 
        public Cursor EditingPanelCursor
        {
            get
            {
                return this.Cursor;
            }
        }
 
        public object GetEditingControlFormattedValue(DataGridViewDataErrorContexts context)
        {
            return EditingControlFormattedValue;
        }
 
        public void PrepareEditingControlForEdit(bool selectAll)
        {
            if (selectAll)
            {
                this.SelectAll();
            }
        }
 
        public bool RepositionEditingControlOnValueChange
        {
            get
            {
                return false;
            }
        }
 
        protected override void OnTextChanged(EventArgs e)
        {
            valueChanged = true;
            this.EditingControlDataGridView.NotifyCurrentCellDirty(true);
            base.OnTextChanged(e);
        }
    }
 
    public class MaskedTextBoxCell : DataGridViewTextBoxCell
    {
        public override Type EditType
        {
            get
            {
                return typeof(MaskedTextBoxEditingControl);
            }
        }
 
        public override void InitializeEditingControl(int rowIndex, object initialFormattedValue, DataGridViewCellStyle dataGridViewCellStyle)
        {
            base.InitializeEditingControl(rowIndex, initialFormattedValue, dataGridViewCellStyle);
                      
 
            if (DataGridView != null && DataGridView.EditingControl is MaskedTextBoxEditingControl)
            {
                ((MaskedTextBoxEditingControl)DataGridView.EditingControl).Mask =
                    this.DataGridView.Columns[this.ColumnIndex].DefaultCellStyle.Format;
            }
        }
        
        public override object DefaultNewRowValue
        {
            get
            {
                return string.Empty;
            }
        }
    }
 
    public class DataGridViewMaskedTextBoxColumn : DataGridViewColumn
    {
        public DataGridViewMaskedTextBoxColumn()
            : base(new MaskedTextBoxCell())
        {
        }
 
        public override DataGridViewCell CellTemplate
        {
            get
            {
                return base.CellTemplate;
            }
            set
            {
                if (value != null &&
                !value.GetType().IsAssignableFrom(typeof(MaskedTextBoxCell)))
                {
                    throw new InvalidCastException("Must be a MaskedTextBoxCell");
                }
                base.CellTemplate = value;
            }
        }
    }