package com.malfunctionz.malfunctionboard.nt.datatypes;

public record MBDouble(String type, double value) implements MBData
{
    public MBDouble(double value)
    {
        this("Double", value);
    }
}
