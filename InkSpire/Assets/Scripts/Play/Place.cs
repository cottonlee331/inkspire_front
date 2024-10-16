using System.Collections.Generic;
using System.Text.RegularExpressions;
using OpenAI;
using System.Threading.Tasks;

public class Place
{
    // 특수문자, 괄호, 점 제거를 위한 정규 표현식
    readonly Regex regex = new("[`~!@#$%^&*()_|+\\-=?;:'\",.<>{}[\\]\\\\/]", RegexOptions.IgnoreCase);

    private List<ChatMessage> gpt_messages = new();
    public int id; //장소 id
    public string place_name = ""; //장소 이름
    public string place_info = ""; //장소 설명
    public int ANPC_exist = 0; //ANPC 등장 여부
    public bool clear; //파싱용 클리어 속성

    public async Task InitPlace(int idx, Script script, Item item, int event_type, List<string> place_names)
    {
        IsANPCexists(event_type);
        await CreatePlace(idx, script, item, place_names);
    }

    //ANPC 미등장 == 0, 등장 == 1 (목표이벤트일 경우 무조건 0)
    public void IsANPCexists(int event_type)
    {
        if (event_type == 1)
        {
            ANPC_exist = 0;
        }
        else
        {
            ANPC_exist = UnityEngine.Random.Range(0, 2);
        }
    }

    public async Task CreatePnpcPlace(Script script, Npc pro_npc)
    {
        string time_background = script.GetTimeBackground();
        string space_background = script.GetSpaceBackground();
        string world_detail = script.GetWorldDetail();
        string genre = script.GetGenre();

        string pnpc_name = pro_npc.GetName();
        string pnpc_detail = pro_npc.GetDetail();

        gpt_messages.Clear();

        ChatMessage prompt_msg = new()
        {
            Role = "system",
            Content = @"당신은 조력자 NPC가 머무는 장소를 제시한다.
다음은 게임의 배경인 " + time_background + "시대 " + space_background + "를 배경으로 하는 세계관에 대한 설명이다. " + world_detail +
            @" 장소는 해당 게임의 조력자 NPC의 집 혹은 직장으로 생성되며 조력자 NPC의 정보는 다음과 같다. " +
            "이름은 " + pnpc_name + "이며, " + pnpc_detail +
            @" 장소 생성 양식은 아래와 같다. 각 줄의 요소는 반드시 모두 포함되어야 하며, 반드시 아래 양식을 따라야 한다. 또한, 출력의 영어표기를 생략하고 한글표기만 나타낸다. ** 이 표시 안의 내용은 문맥에 맞게 채운다.
ex)
장소명: *장소 이름을 한 단어로 출력*
장소설명: *장소에 대한 설명을 50자 내외로 설명, 어미는 입니다 체로 통일합니다.* "
        };
        gpt_messages.Add(prompt_msg);

        var query_msg = new ChatMessage()
        {
            Role = "user",
            Content = "진행중인 게임의 " + genre + " 장르와 세계관에 어울리는 장소 생성."
        };

        gpt_messages.Add(query_msg);
        StringToPlace(await GptManager.gpt.CallGpt(gpt_messages));
    }

    public async Task CreatePlace(int idx, Script script, Item item, List<string> place_names)
    {
        string time_background = script.GetTimeBackground();
        string space_background = script.GetSpaceBackground();
        string world_detail = script.GetWorldDetail();
        string genre = script.GetGenre();

        gpt_messages.Clear();

        ChatMessage prompt_msg;

        prompt_msg = new ChatMessage()
        {
            Role = "system",
            Content = @"당신은 게임 진행에 필요한 장소를 제시한다.
            다음은 게임의 배경인 " + time_background + "시대" + space_background + "를 배경으로 하는 세계관에 대한 설명이다." + world_detail
        };
        if (item.type != ItemType.Null) {
            prompt_msg.Content += @"다음은 이 장소에서 발견할 수 있는 아이템에 대한 설명이다. 
아이템 이름: " + item.name + @"
아이템 설명: " + item.info;
        }
        prompt_msg.Content += @"장소는 게임의 배경에 맞추어 플레이어가 흥미롭게 탐색할 수 있는 곳으로 생성된다. 장소 생성 양식은 아래와 같다. 각 줄의 요소는 반드시 모두 포함되어야 하며, 반드시 아래 생성 양식을 따라야 한다. ** 이 표시 안의 내용은 문맥에 맞게 채운다.
ex)
장소명: *장소 이름을 한 단어로 출력*
장소설명: *장소에 대한 설명을 50자 내외로 설명, 어미는 입니다 체로 통일합니다.*";
        gpt_messages.Add(prompt_msg);

        var query_msg = new ChatMessage()
        {
            Role = "user",
            Content = "와 장소 이름이 중복되지 않도록 진행중인 게임의 " + genre + " 장르와 세계관에 어울리는 장소 생성. 장소 이름은 절대 중복되어서는 안된다."
        };
        for (int i = 0; i < idx; i++)
        {
            if (i != 0)
            {
                query_msg.Content = place_names[i] + ", " + query_msg.Content;
            }
            else
            {
                query_msg.Content = place_names[i] + query_msg.Content;
            }
        }

        gpt_messages.Add(query_msg);
        StringToPlace("출력 " + await GptManager.gpt.CallGpt(gpt_messages));
    }

    //장소 이름 및 장소 설명 파싱 함수
    public void StringToPlace(string place_string)
    {
        clear = false;

        string[] place_arr;
        place_string = place_string.Replace("장소명: ", "#");
        place_string = place_string.Replace("장소설명: ", "#");
        place_string = place_string.Replace("장소명:", "#");
        place_string = place_string.Replace("장소설명:", "#");

        place_arr = place_string.Split('#');
        place_name = regex.Replace(place_arr[1].Trim('\n'), "");
        place_info = place_arr[2].Trim('\n');
    }

    public void SetClear(bool clr)
    {
        clear = clr;
    }

    public void SetMapInfo(GetMapInfo map_info)
    {
        id = map_info.mapId;
        place_name = map_info.name;
        place_info = map_info.info;
        ANPC_exist = map_info.anpc ? 1 : 0;
    }

    public void SetMapId(int map_id)
    {
        id = map_id;
    }
}
