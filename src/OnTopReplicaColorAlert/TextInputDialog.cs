using System.Drawing;
using System.Windows.Forms;

namespace OnTopReplicaColorAlert {

    /// <summary>
    /// Minimal dialog asking for a single line of text.
    /// </summary>
    sealed class TextInputDialog : Form {

        readonly TextBox _textBox;

        public TextInputDialog(string title, string prompt) {
            Text = title;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            //The panels are topmost: stay above them when shown without owner
            TopMost = true;
            AutoScaleMode = AutoScaleMode.Font;
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            Padding = new Padding(10);

            var layout = new TableLayoutPanel {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                ColumnCount = 1,
                Dock = DockStyle.Fill
            };

            var label = new Label {
                Text = prompt,
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 6)
            };
            _textBox = new TextBox {
                Width = 320,
                Margin = new Padding(0, 0, 0, 10)
            };

            var buttons = new FlowLayoutPanel {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.RightToLeft,
                Dock = DockStyle.Fill,
                Margin = Padding.Empty
            };
            var cancel = new Button {
                Text = Strings.DialogCancel,
                DialogResult = DialogResult.Cancel,
                AutoSize = true
            };
            var ok = new Button {
                Text = Strings.DialogOK,
                DialogResult = DialogResult.OK,
                AutoSize = true
            };
            buttons.Controls.Add(cancel);
            buttons.Controls.Add(ok);

            layout.Controls.Add(label);
            layout.Controls.Add(_textBox);
            layout.Controls.Add(buttons);
            Controls.Add(layout);

            AcceptButton = ok;
            CancelButton = cancel;
            MinimumSize = new Size(300, 0);
        }

        /// <summary>
        /// Gets the text entered by the user.
        /// </summary>
        public string Value => _textBox.Text.Trim();
    }
}
