package com.malfunctionz.malfunctionboard.nt.datatypes;

public record MBString(String type, String data)
{
    public MBString(String data)
    {
        this("String", data);
    }
}
