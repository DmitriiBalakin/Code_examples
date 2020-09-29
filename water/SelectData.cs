using System;

public class SelectData
{
    public readonly string Value;
    public readonly string Text;
    public readonly string Code;
    public readonly string buk;
    public readonly int Ved;

    public SelectData(string Value, string Text)
    {
        this.Value = Value;
        this.Text = Text;
    }

    public SelectData(string Value, string Text, int Ved)
    {
        this.Value = Value;
        this.Text = Text;
        this.Ved = Ved;
    }


    public SelectData(string buk, string Value, string Text)
    {
        this.buk = buk;
        this.Value = Value;
        this.Text = Text;
    }

    public override string ToString()
    {
        return this.Text;
    }
}
