

public struct WaitData
{
    public string Name;
    public int Rating;
    public Guid Id;
}

public class MatchMaker
{
    public Server server;
    public List<WaitData> Waiting = [];
    public int playerCount;
    

    public MatchMaker(Server sr)
    {
        server = sr;
    }

    public WaitData[] FindMatching(WaitData data)
    {
        WaitData[] output = new WaitData[playerCount];
        output[0] = data;
        float[] matching = new float[playerCount];
        for (int i = 0; i < playerCount; i++)
            output[i].Rating = -1;
        int[] indexes = new int[playerCount];
        for (int i = 0; i < playerCount; i++)
            indexes[i] = -1;

        float currMatching;
        for (int i = 0; i < playerCount; i++)
        {
            for (int j = 0; j < Waiting.Count; j++)
            {
                if (Waiting[j].Id == data.Id)
                {
                    output[i] = Waiting[j];
                    indexes[i] = j;
                    break;
                }
                currMatching = Difference(data, Waiting[j]);
                if (currMatching < matching[i] || output[i].Rating == -1)
                {
                    output[i] = Waiting[j];
                    indexes[i] = j;
                    matching[i] = currMatching;
                }
            }
            Waiting.RemoveAt(indexes[i]);
        }

        /*for (int j = 0; j < playerCount; j++)
        {
            Waiting.RemoveAt(indexes[j] - j);
        }*/

        return output;
    }

    public void AddToQueue(WaitData data)
    {
        if (Waiting.Exists(value => value.Id == data.Id))
            return;
        Waiting.Add(data);

        if (!HasEnough())
            return;

        WaitData[] matches = FindMatching(data);

        server.CreateRoom(matches, playerCount);
    }

    public void RemoveFromQueue(Guid id)
    {
        for (int i = Waiting.Count - 1; i >= 0; i--)
        {
            if (Waiting[i].Id == id)
            {
                Waiting.RemoveAt(i);
            }
        }
    }

    public bool HasEnough()
    {
        return Waiting.Count >= playerCount;
    }

    private float Difference(WaitData first, WaitData second)
    {
        return MathF.Abs(first.Rating - second.Rating);
    }
}
