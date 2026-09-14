package com.malfunctionz.malfunctionboard.nt;

import com.google.gson.FieldNamingPolicy;
import com.google.gson.Gson;
import com.google.gson.GsonBuilder;
import com.malfunctionz.malfunctionboard.nt.datatypes.*;
import edu.wpi.first.networktables.NetworkTable;
import edu.wpi.first.networktables.NetworkTableInstance;
import edu.wpi.first.networktables.StringPublisher;
import java.util.HashMap;

public class MalfunctionBoard
{
    static final String kNetworkTableName = "MalfunctionBoardTable";
    static final Gson gson = new GsonBuilder()
        .setFieldNamingPolicy(FieldNamingPolicy.UPPER_CAMEL_CASE)
        .create();

    final NetworkTable networkTable;
    final HashMap<String, StringPublisher> publisherCache = new HashMap<>();

    public MalfunctionBoard()
    {
        networkTable = NetworkTableInstance.getDefault().getTable(kNetworkTableName);
    }

    public void writeData(String entryName, MBData data)
    {
        writeDataToEntry(getEntry(entryName), data);
    }

    StringPublisher getEntry(String entryName)
    {
        return publisherCache.computeIfAbsent(entryName, name -> networkTable.getStringTopic(name).publish());
    }

    static void writeDataToEntry(StringPublisher entry, MBData data)
    {
        String json = gson.toJson(data);
        entry.set(json);
    }

    public void close()
    {
        for (StringPublisher publisher : publisherCache.values())
        {
            publisher.close();
        }
        publisherCache.clear();
    }
}
