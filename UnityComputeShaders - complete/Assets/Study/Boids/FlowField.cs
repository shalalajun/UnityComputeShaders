using UnityEngine;

public class FlowField : MonoBehaviour
{
    
    public float resolution = 1.0f;

    public int cols, rows;
    private Vector2[,] field;


    void Start()
    {
        if (cols == 0) cols = 10;
        if (rows == 0) rows = 10;

        field = new Vector2[cols,rows];

        Init();
        OnDrawGizmos();
    }
    void Init()
    {
        float xOffSeed = Random.Range(0f, 10000f);
        float yOffSeed = Random.Range(0f, 10000f);

        float xOff = xOffSeed;

        for(int i=0; i<cols; i++){
            float yOff = yOffSeed;
            for(int j=0; j<rows; j++)
            {
                float noiseValue = Mathf.PerlinNoise(xOff, yOff);

                float theta = noiseValue * Mathf.PI * 2;

                field[i, j] = new Vector2(Mathf.Cos(theta), Mathf.Sin(theta));

                yOff += 0.1f;
            }
            xOff += 0.1f;
        }

    }

    void OnDrawGizmos()
        {
            // field가 아직 안 만들어졌으면(실행 전이면) 그리지 마라 (에러 방지)
            if (field == null) return;

            Gizmos.color = Color.white; // 선 색깔 하얀색으로 설정

            for (int i = 0; i < cols; i++)
            {
                for (int j = 0; j < rows; j++)
                {
                    // 1. 이 화살표가 그려질 시작 위치 계산
                    // (배열의 칸 번호 x 한 칸의 크기)
                    Vector2 drawPos = new Vector2(i * resolution, j * resolution);

                    // 2. 그 위치에 저장된 벡터(방향) 가져오기
                    Vector2 direction = field[i, j];

                    // 3. 선 긋기 (시작위치, 방향 * 길이)
                    // direction에 0.9를 곱한 건 칸을 꽉 채우지 말고 약간 여백을 두려고 줄인 겁니다.
                    Gizmos.DrawRay(drawPos, direction * 0.9f * resolution);
                    
                    // (선택사항) 화살표 머리 대신 끝에 작은 원을 그려서 방향 확인
                    Gizmos.DrawSphere(drawPos + direction * 0.5f, 0.1f);
                }
            }
        }
    // Vehicle이 자신의 위치(position)를 주면, 그곳의 흐름(Vector2)을 반환하는 함수
    public Vector2 Lookup(Vector3 position)
    {
        // 1. 월드 좌표를 그리드 좌표(몇 번째 칸)로 변환
        // 예: 위치가 5.5이고 해상도가 1이면 -> 5번째 칸
        int column = (int)Mathf.Clamp(position.x / resolution, 0, cols - 1);
        int row = (int)Mathf.Clamp(position.y / resolution, 0, rows - 1);

        // 2. 그 칸에 저장된 벡터 반환
        return field[column, row];
    }
}
