package com.malfunctionz.malfunctionboard.nt.datatypes;

public record MBDouble(String type, double value)
{
    public MBDouble(double value)
    {
        this("Double", value);
    }
}
