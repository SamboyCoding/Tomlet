using System;

namespace Tomlet;

public class TomletStringReader : IDisposable
{
    private string? _s;
    private int _pos;
    private int _length;

    public TomletStringReader(string s)
    {
        _s = s;
        _length = s.Length;
    }

    public void Backtrack(int amount)
    {
        if(_pos < amount)
            throw new("Cannot backtrack past the beginning of the string");
        
        _pos -= amount;
    }

    public void Dispose()
    {
        _s = null;
        _pos = 0;
        _length = 0;
    }

    public int Peek() => _pos == _length ? -1 : _s![_pos];

    public int Read() => _pos == _length ? -1 : _s![_pos++];

    public int Read(char[] buffer, int index, int count)
    {
        var remainingReadable = _length - _pos;
        if (remainingReadable <= 0) 
            return remainingReadable;
        
        if (remainingReadable > count)
            remainingReadable = count;
        
        _s!.CopyTo(_pos, buffer, index, remainingReadable);
        _pos += remainingReadable;
        return remainingReadable;
    }
    
    public int ReadBlock(char[] buffer, int index, int count)
    {
        var numRead = 0;
        int lastRead;
        do
        {
            numRead += lastRead = Read(buffer, index + numRead, count - numRead);
        }
        while (lastRead > 0 && numRead < count);
        
        return numRead;
    }
}