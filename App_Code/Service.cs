using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text;

public class Service :IServiceRest
{
    #region Metodos para verificar se o serviço está ativo e funcional

    public IsAlive GetState()
    {
        return new IsAlive(true);
    }

    #endregion

    #region Operações Cruid de ligação a Base de Dados SQL definida no Web.Config

    private DataSet ExecuteQuery(string query, SqlParameter[] parametrosSQL = null)
    {
        try
        {
            //Abre Conexao
            SqlConnection conn = new SqlConnection();
            conn.ConnectionString = ConfigurationManager.ConnectionStrings["connectionSQLServer"].ConnectionString;

            //Prepara a Consulta
            SqlDataAdapter adaptadorSQL = new SqlDataAdapter(query, conn);

            //Adiciona Parametros
            if (parametrosSQL != null)
            {
                foreach (SqlParameter parametro in parametrosSQL)
                    adaptadorSQL.SelectCommand.Parameters.Add(parametro);
            }

            //Executa SELECT e guarda os dados numa DataSet
            DataSet dados = new DataSet();
            adaptadorSQL.Fill(dados);

            //retorna Dados caso seja SELCT
            return dados;
        }
        catch (Exception ex)
        {
            return null;
        }
    }

    #endregion

    #region Metodo Retornar Hora Servidor

    public DataHora GetDateTime()
    {
        return new DataHora(DateTime.Now);
    }

    #endregion

    #region Extrai Lista dos Postos Picagem, Tempos Definidos Ações sobre faltas,Estado Picagem, Operador. Retifica e Regista Nova Picagem

    public List<PicagemDetalhes> GetListPicks(string dataIni, string dataFim, string operador, string posto)
    {
        //VARIAVEIS
        int i = 0, parametros = 0, parametrosaux = 0,periodo=0,turno=0, estado = 0;
        DateTime horaPicagem, horaTurnoNormalIni, horaTurnoNormalFim; ;
        TimeSpan horaTolerancia, dataApoio, horaTurno,horaAlmoçoNormal;
        StringBuilder query = new StringBuilder();
        SqlParameter[] parametroSQL = null;

        query.AppendLine("SELECT  ");
        query.AppendLine("		PIOperadores.Operador,  ");
        query.AppendLine("		Nome, ");
        query.AppendLine("		PITurnos.ID AS IDTurno,	");
        query.AppendLine("		PITurnos.Descr AS Turno, ");
        query.AppendLine("		PIPostosPicagem.Descr AS Posto, ");
        query.AppendLine("		CONVERT(DATE,Data) AS DataPicagem, ");
        query.AppendLine("		CONVERT(TIME(7),Data) AS HoraPicagem,  ");
        query.AppendLine("		DataIni AS HoraTurno, ");
        query.AppendLine("		PITurnos.ToleranciaPicagem, ");
        query.AppendLine("		PITurnos.NumeroPicagens AS NumeroPicagensEsperadas, ");
        query.AppendLine("		T2.NumeroPicagemRegistadas, ");
        query.AppendLine("		Automatica, ");
        query.AppendLine("		TimeIni AS ToleranciaNormalIni, ");
        query.AppendLine("		TimeFim AS ToleranciaNormalFim, ");
        query.AppendLine("		PIPicagens.Periodo AS Periodo, ");
        query.AppendLine("		TempoAlmoco ");
        query.AppendLine("FROM PIPicagens ");
        query.AppendLine("INNER JOIN PIOperadores ON PIPicagens.Operador = PIOperadores.Operador ");
        query.AppendLine("INNER JOIN PIHorarios ON PIOperadores.Turno = PIHorarios.Turno AND PIPicagens.Periodo=PIHorarios.Periodo ");
        query.AppendLine("INNER JOIN PITurnos ON PIOperadores.Turno = PITurnos.ID ");
        query.AppendLine("INNER JOIN PIPostosPicagem ON PIPicagens.Posto = PIPostosPicagem.Posto ");
        query.AppendLine("LEFT JOIN PITurnoNormal ON  PITurnoNormal.Turno=PIOperadores.Turno, ");
        query.AppendLine("( ");
        query.AppendLine("	SELECT  ");
        query.AppendLine("		   PIPicagens.Operador,  ");
        query.AppendLine("		   CONVERT(DATE,Data) AS DataPicagem, ");
        query.AppendLine("		   COUNT(*) AS NumeroPicagemRegistadas ");
        query.AppendLine("	FROM PIPicagens ");
        query.AppendLine("	GROUP BY PIPicagens.Operador, CONVERT(DATE,Data) ");
        query.AppendLine(") AS T2 ");
        query.AppendLine("WHERE T2.Operador=PIPicagens.Operador AND T2.DataPicagem=CONVERT(DATE,Data) ");
       

        //PARAMETROS INVOCAÇÂO METODO
        if (dataIni != null && dataIni.ToUpper() == "NULL") dataIni = null;
        if (dataFim != null && dataFim.ToUpper() == "NULL") dataFim = null;
        if (operador != null && operador.ToUpper() == "NULL") operador = null;
        if (posto != null && posto.ToUpper() == "NULL") posto = null;
        if (dataIni != null) parametros++;
        if (dataFim != null) parametros++;
        if (operador != null) parametros++;
        if (posto != null) parametros++;

        //PRAMETROS SQL
        if (parametros > 0) { parametroSQL = new SqlParameter[parametros]; }
        if (parametros > 0) parametrosaux = parametros;
        if (dataIni != null) { parametroSQL[parametros - parametrosaux] = new SqlParameter("@dataIni", dataIni); query.AppendLine(" AND T2.DataPicagem>=@dataIni "); parametrosaux--; }
        if (dataFim != null) { parametroSQL[parametros - parametrosaux] = new SqlParameter("@dataFim", dataFim); query.AppendLine(" AND T2.DataPicagem<=@dataFim "); parametrosaux--; }
        if (operador != null) { parametroSQL[parametros - parametrosaux] = new SqlParameter("@operador", operador); query.AppendLine(" AND PIOperadores.Operador=@operador "); parametrosaux--; }
        if (posto != null) { parametroSQL[parametros - parametrosaux] = new SqlParameter("@posto", posto); query.AppendLine(" AND PIPostosPicagem.Descr=@posto  "); parametrosaux--; }
        //if (parametros > 0) query.Remove(query.Length - 3, 3);
        query.AppendLine("ORDER BY DataPicagem,PIOperadores.Operador,HoraPicagem ");

        //EXECUTA DADOS E COLOCA NUMA LISTA 
        DataSet dados = ExecuteQuery(query.ToString(), parametroSQL);
        if (dados == null)
            return null;
        PicagemDetalhes[] picagens = new PicagemDetalhes[dados.Tables[0].Rows.Count];
        foreach (DataRow linha in dados.Tables[0].Rows)
        {
            //VARIAVEIS DE APOIO
            estado = 0;
            dataApoio = TimeSpan.MinValue;
            horaPicagem = Convert.ToDateTime(linha["HoraPicagem"].ToString());
            horaTurno = Convert.ToDateTime(linha["HoraTurno"].ToString()).TimeOfDay;
            horaTolerancia = Convert.ToDateTime(linha["ToleranciaPicagem"].ToString()).TimeOfDay;
            horaTurnoNormalIni = (linha["ToleranciaNormalIni"].ToString()!="")? Convert.ToDateTime(linha["ToleranciaNormalIni"].ToString()):DateTime.MinValue;
            horaTurnoNormalFim = (linha["ToleranciaNormalFim"].ToString() != "") ? Convert.ToDateTime(linha["ToleranciaNormalFim"].ToString()) : DateTime.MinValue;
            horaAlmoçoNormal = (linha["TempoAlmoco"].ToString() != "") ? Convert.ToDateTime(linha["TempoAlmoco"].ToString()).TimeOfDay : TimeSpan.MinValue;
            periodo = int.Parse(linha["Periodo"].ToString());
            turno = int.Parse(linha["IDTurno"].ToString());

            //SE O TURNO 1/2 CALCULA AS TOLERANCIAS
            if(turno==1 || turno==2)
            {
                dataApoio = horaTurno;
                if (periodo == 1)
                    dataApoio -= horaTolerancia;
                else if (periodo==2)
                    dataApoio += horaTolerancia;
            }

            //CALCULA TORLERANCIAS TURNO NORMAL
            if (turno == 4)
            {
                dataApoio = horaTurno;
                if (periodo == 1)
                    dataApoio -= horaTolerancia;
                else if (periodo == 2)
                    dataApoio = horaTurnoNormalIni.TimeOfDay;
                else if (periodo == 3)
                    dataApoio = horaTurnoNormalFim.TimeOfDay;
                else if (periodo == 4)
                    dataApoio += horaTolerancia;
            }

            if (linha["NumeroPicagensEsperadas"].ToString() != linha["NumeroPicagemRegistadas"].ToString())
                estado =1 ; //Falta Registo Picagem

            if (linha["Automatica"].ToString() != "True")
                estado =2; //Picagem Assinalada Manualmente

            if (turno == 1 || turno == 2)
            {
                if (periodo == 1)
                {
                    if (!(horaPicagem.TimeOfDay >= dataApoio && horaPicagem.TimeOfDay <= horaTurno))
                        estado = 3; //Picagem Entrada Fora do Horario Esperado
                }
                else if (periodo == 2)
                {
                    if (!(horaPicagem.TimeOfDay >= horaTurno && horaPicagem.TimeOfDay <= dataApoio))
                        estado = 4; //Picagem Saida Fora do Horario Esperado
                }
            }

            if (turno == 4) // falata verificar tempo hora almoço
            {
                if (periodo == 1)
                {
                    if (!(horaPicagem.TimeOfDay >= dataApoio && horaPicagem.TimeOfDay <= horaTurno))
                        estado = 3; //Picagem Entrada Fora do Horario Esperado
                }
                else if (periodo == 2)
                {
                    if (horaPicagem.TimeOfDay < horaTurnoNormalIni.TimeOfDay)
                        estado =5; //Picagem almoço antes do esperado
                    else if (horaPicagem.TimeOfDay > horaTurnoNormalFim.TimeOfDay)
                        estado =6; //Picagem almoço depois do esperado
                }
                else if (periodo == 3)
                {
                    if (horaPicagem.TimeOfDay < horaTurnoNormalIni.TimeOfDay)
                        estado = 7; //Picagem almoço antes do esperado
                    else if (horaPicagem.TimeOfDay > horaTurnoNormalFim.TimeOfDay)
                        estado = 8; //Picagem almoço depois do esperado
                }
                else if (periodo == 4)
                {
                    if (!(horaPicagem.TimeOfDay >= horaTurno && horaPicagem.TimeOfDay <= dataApoio))
                        estado = 4; //Picagem Saida Fora do Horario Esperado
                }
            }

            //CRIAR UMA NOVA CLASS COM OS DADOS CALCULADOS ACIMA
            picagens[i] = new PicagemDetalhes(
                int.Parse(linha["Operador"].ToString()),
                linha["Nome"].ToString(),
                linha["Turno"].ToString(),
                linha["Posto"].ToString(),
                Convert.ToDateTime(linha["DataPicagem"].ToString()),
                Convert.ToDateTime(linha["HoraPicagem"].ToString()),
                estado
                );
            i++;
        }   

        List<PicagemDetalhes> aux = new List<PicagemDetalhes>();
        aux.AddRange(picagens);

        //RETORNA LISTA
        return aux;
    }
 
    public List<TurnoTempos> GetListTimming()
    {
        StringBuilder query = new StringBuilder();
        DataSet dados;
        int i = 0;

        query.AppendLine("SELECT Descr,DataIni,ToleranciaPicagem,TimeIni,TimeFim,TempoAlmoco  ");
        query.AppendLine("FROM PITurnos  ");
        query.AppendLine("LEFT JOIN PITurnoNormal ON PITurnos.ID = PITurnoNormal.Turno  ");
        query.AppendLine("LEFT JOIN PIHorarios ON PITurnos.ID = PIHorarios.Turno ");
        dados = ExecuteQuery(query.ToString());
        if (dados == null)
            return null;
        TurnoTempos[] tempos = new TurnoTempos[dados.Tables[0].Rows.Count];

        foreach (DataRow linha in dados.Tables[0].Rows)
        {
            tempos[i] = new TurnoTempos(
                linha["Descr"].ToString(),
                (linha["DataIni"]!=DBNull.Value)?Convert.ToDateTime(linha["DataIni"].ToString()):DateTime.MinValue,
                (linha["ToleranciaPicagem"] != DBNull.Value) ? Convert.ToDateTime(linha["ToleranciaPicagem"].ToString()) : DateTime.MinValue,
                (linha["TimeIni"] != DBNull.Value) ? Convert.ToDateTime(linha["TimeIni"].ToString()) : DateTime.MinValue,
                (linha["TimeFim"] != DBNull.Value) ? Convert.ToDateTime(linha["TimeFim"].ToString()) : DateTime.MinValue,
                (linha["TempoAlmoco"] != DBNull.Value) ? Convert.ToDateTime(linha["TempoAlmoco"].ToString()) : DateTime.MinValue
                );
            i++;
        }

        List<TurnoTempos> aux = new List<TurnoTempos>();
        aux.AddRange(tempos);

        //RETORNA LISTA
        return aux;
    }

    public List<PostoPicagem> GetListPosts()
    {
        //VARIAVEIS
        int i = 0;
        StringBuilder query = new StringBuilder();
        SqlParameter[] parametroSQL = null;
        query.AppendLine("SELECT * FROM PIPostosPicagem ORDER BY Posto");

        //EXECUTA DADOS E COLOCA NUMA LISTA 
        DataSet dados = ExecuteQuery(query.ToString(), parametroSQL);
        if (dados == null)
            return null;
        PostoPicagem[] posto = new PostoPicagem[dados.Tables[0].Rows.Count];
        foreach (DataRow linha in dados.Tables[0].Rows)
        {
            posto[i] = new PostoPicagem(Convert.ToInt32(linha[0]), linha[1].ToString());
            i++;
        }
        List<PostoPicagem> aux = new List<PostoPicagem>();
        aux.AddRange(posto);

        //RETORNA LISTA
        return aux;
    }

    public List<AccaoSobrePicagem> GetListAction()
    {
        //VARIAVEIS
        int i = 0;
        StringBuilder query = new StringBuilder();
        SqlParameter[] parametroSQL = null;
        query.AppendLine("SELECT * FROM  PIPicagensCausasJustificacao ORDER BY ID");

        //EXECUTA DADOS E COLOCA NUMA LISTA 
        DataSet dados = ExecuteQuery(query.ToString(), parametroSQL);
        if (dados == null)
            return null;
        AccaoSobrePicagem[] acc = new AccaoSobrePicagem[dados.Tables[0].Rows.Count];
        foreach (DataRow linha in dados.Tables[0].Rows)
        {
            acc[i] = new AccaoSobrePicagem(Convert.ToInt32(linha[0]), linha[1].ToString());
            i++;
        }
        List<AccaoSobrePicagem> aux = new List<AccaoSobrePicagem>();
        aux.AddRange(acc);

        //RETORNA LISTA
        return aux;
    }

    public List<PicagemEstado> GetListStatePick()
    {
        //VARIAVEIS
        int i = 0;
        StringBuilder query = new StringBuilder();
        SqlParameter[] parametroSQL = null;
        query.AppendLine("SELECT * FROM  PIPicagensEstados ORDER BY ID");

        //EXECUTA DADOS E COLOCA NUMA LISTA 
        DataSet dados = ExecuteQuery(query.ToString(), parametroSQL);
        if (dados == null)
            return null;
        PicagemEstado[] acc = new PicagemEstado[dados.Tables[0].Rows.Count];
        foreach (DataRow linha in dados.Tables[0].Rows)
        {
            acc[i] = new PicagemEstado(Convert.ToInt32(linha[0]), linha[1].ToString());
            i++;
        }
        List<PicagemEstado> aux = new List<PicagemEstado>();
        aux.AddRange(acc);

        //RETORNA LISTA
        return aux;
    }

    public Operador GetOperator(string numero)
    {

        //VARIAVEIS

        StringBuilder query = new StringBuilder();
        SqlParameter[] parametroSQL = new SqlParameter[1];

        query.AppendLine("SELECT Operador, Nome, Descr ");
        query.AppendLine("FROM Pioperadores ");
        query.AppendLine("INNER JOIN PITurnos ON PIOperadores.Turno = PITurnos.ID  ");
        query.AppendLine("WHERE Operador=@numero");

        parametroSQL[0] = new SqlParameter("@numero", numero);

        //EXECUTA DADOS E COLOCA NUMA LISTA 
        DataSet dados = ExecuteQuery(query.ToString(), parametroSQL);
        if (dados == null)
            return null;

        foreach (DataRow linha in dados.Tables[0].Rows)
        {
            return new Operador(
                        int.Parse(linha["Operador"].ToString()),
                        linha["Nome"].ToString(),
                        linha["Descr"].ToString()
                        );
        }

        return null;
    }

    public PicagemJustificacao GetJustificationPick(string operador,string dataNova)
    {
        DateTime data = Convert.ToDateTime(dataNova);
        StringBuilder query = new StringBuilder();
        SqlParameter[] parametroSQL = new SqlParameter[2];

        query.AppendLine("SELECT PIPicagensJustificacao.Operador,DataAntiga,DataNova,UtilCria,PIPicagensJustificacao.DataCria as DataCria,Nome,Obs,Causa ");
        query.AppendLine("FROM PIPicagens,PIPicagensJustificacao,PIOperadores ");
        query.AppendLine("WHERE PIPicagensJustificacao.Operador = PIPicagens.Operador AND Data = DataNova AND ");
        query.AppendLine("    PIOperadores.Operador = PIPicagensJustificacao.UtilCria AND ");
        query.AppendLine("    PIPicagensJustificacao.Operador = @Operador AND CONVERT(DATE,PIPicagens.Data) = @Data ");

        parametroSQL[0] = new SqlParameter("@Operador", operador);
        parametroSQL[1] = new SqlParameter("@Data", data);

        //EXECUTA DADOS E COLOCA NUMA LISTA 
        DataSet dados = ExecuteQuery(query.ToString(), parametroSQL);
        if (dados == null)
            return null;

        foreach (DataRow linha in dados.Tables[0].Rows)
        {
            return new PicagemJustificacao(
                        int.Parse(linha["Operador"].ToString()),
                        int.Parse(linha["UtilCria"].ToString()),
                        linha["Nome"].ToString(),
                        Convert.ToDateTime(linha["DataAntiga"].ToString()),
                        Convert.ToDateTime(linha["DataNova"].ToString()),
                        linha["Obs"].ToString(),
                        linha["Causa"].ToString(),
                        false,
                        Convert.ToDateTime(linha["DataCria"].ToString())
                        );
        }
        return null;
    }

    public bool PostInsertRetifyPick(PicagemJustificacao pic)
    {
        StringBuilder query = new StringBuilder();
        SqlParameter[] parametroSQL;
        DataSet dados;
        pic.DataNew = pic.DataNew.AddHours(1);
        pic.DataOld= pic.DataOld.AddHours(1);
        
        if(!pic.Novo)
        {
            parametroSQL = new SqlParameter[6];
            query.AppendLine("INSERT INTO PIPicagensJustificacao (Operador,DataAntiga,DataNova,Causa,Obs,UtilCria)");
            query.AppendLine("VALUES(@Operador, @DataAntiga, @DataNova, @Causa, @Obs, @UtilCria)");
            parametroSQL[0] = new SqlParameter("@Operador", pic.Op);
            parametroSQL[1] = new SqlParameter("@DataAntiga", pic.DataOld);
            parametroSQL[2] = new SqlParameter("@DataNova", pic.DataNew);
            parametroSQL[3] = new SqlParameter("@Causa", pic.Causa);
            parametroSQL[4] = new SqlParameter("@Obs", pic.Obs);
            parametroSQL[5] = new SqlParameter("@UtilCria", pic.OpJust);
            dados = ExecuteQuery(query.ToString(), parametroSQL);
            if (dados == null)
                return false;
            query = new StringBuilder();
            parametroSQL = new SqlParameter[3];
            query.AppendLine("UPDATE PIPicagens SET Data=@DataNova,Automatica=0 Tipo=5 ");
            query.AppendLine("WHERE Data=@DataAntiga AND Operador=@Operador");
            parametroSQL[0] = new SqlParameter("@Operador", pic.Op);
            parametroSQL[1] = new SqlParameter("@DataAntiga", pic.DataOld);
            parametroSQL[2] = new SqlParameter("@DataNova", pic.DataNew);
            dados = ExecuteQuery(query.ToString(), parametroSQL);
            if (dados == null)
                return false;
        }
        else
        {
            parametroSQL = new SqlParameter[6];
            query.AppendLine("INSERT INTO PIPicagensJustificacao (Operador,DataAntiga,DataNova,Causa,Obs,UtilCria) ");
            query.AppendLine("VALUES(@Operador, @DataAntiga, @DataNova, @Causa, @Obs, @UtilCria)");
            parametroSQL[0] = new SqlParameter("@Operador", pic.Op);
            parametroSQL[1] = new SqlParameter("@DataAntiga", pic.DataOld);
            parametroSQL[2] = new SqlParameter("@DataNova", pic.DataNew);
            parametroSQL[3] = new SqlParameter("@Causa", pic.Causa);
            parametroSQL[4] = new SqlParameter("@Obs", pic.Obs);
            parametroSQL[5] = new SqlParameter("@UtilCria", pic.OpJust);
            dados = ExecuteQuery(query.ToString(), parametroSQL);
            if (dados == null)
                return false;
            query = new StringBuilder();
            parametroSQL = new SqlParameter[3];
            query.AppendLine("INSERT INTO PIPicagens (Operador,Data,Posto,Automatica,Periodo,Tipo) ");
            query.AppendLine("VALUES (@Operador,@Data,100,0,@Periodo,5)");
            parametroSQL[0] = new SqlParameter("@Operador", pic.Op);
            parametroSQL[1] = new SqlParameter("@Data", pic.DataNew);
            parametroSQL[2] = new SqlParameter("@Periodo", PeriodNew(pic.Op,pic.DataNew));
            dados = ExecuteQuery(query.ToString(), parametroSQL);
            if (dados == null)
                return false;
        }
        return true;
    }

    #endregion

    #region Efetua Sincronismo entre picagens

    public bool GetSynchronizationPicks() 
    {
        //StringBuilder query = new StringBuilder();
        //SqlParameter[] parametroSQL = new SqlParameter[1];
        //DataSet dados = new DataSet();
        //string ficheiro = ConfigurationManager.ConnectionStrings["connectionServerPicagem"].ToString();
        //string nome = "BAK\\INPUT_"+ DateTime.Now.ToString() + ".txt";
        //nome = nome.Replace('/', '_');
        //nome = nome.Replace(':', '_');

        //FileInfo ficheiroAoio = new FileInfo(ficheiro + "INPUT_TRATA.txt");
        //if (ficheiroAoio.Exists)
        //    ficheiroAoio.Delete();

        //FileInfo ficheiroNPica = new FileInfo(ficheiro + "INPUT.txt");
        //ficheiroNPica.CopyTo(ficheiro + "INPUT_TRATA.txt");
        //ficheiroNPica.CopyTo(ficheiro + nome);
        //ficheiroNPica.Delete();
        ////File.Create(ficheiro + "INPUT.txt");
        //File.Open(ficheiro + "INPUT.txt", FileMode.Create).Close();

        var lines = File.ReadAllLines("Picking/INPUT.txt");
        return true;

        ////VERIFICA SE O FICHEIRO EXISTE
        //FileInfo ficheiroPica = new FileInfo(ficheiro + "INPUT_TRATA.txt");
        //if (!ficheiroPica.Exists)
        //    return false;
        
        ////PERCORRE TODAS AS LINHAS DO FICHEIRO DE TEXO
        //foreach (var line in lines)
        //{
        //    //Verifica se ja importou a picagem
        //    parametroSQL[0] = new SqlParameter("@Picagem", line.ToString());
        //    query.AppendLine("SELECT ID  FROM PIPicagensInput WHERE Picagem = @Picagem");
        //    dados = ExecuteQuery(query.ToString(), parametroSQL);
        //    if (dados == null)
        //        return false;
        //    query.Clear();

        //    //Caso a Picagem não tenha sido importada importa novamente.
        //    parametroSQL[0] = new SqlParameter("@Picagem", line.ToString());
        //    query.AppendLine("INSERT INTO PIPicagensInput (Picagem,Tipo,UtilCria) VALUES (@Picagem,1,10000)");
        //    if (dados.Tables[0].Rows.Count == 0)
        //        ExecuteQuery(query.ToString(),parametroSQL);
        //    query.Clear();
        //}

        //if (Synchronization())
        //    return true;
        //else
        //    return false;
    }

    private bool Synchronization()
    {
        StringBuilder query = new StringBuilder();
        SqlParameter[] paramSQL = new SqlParameter[2];
        List<PostoPicagem> postoPicagem = GetListPosts();
        Operador op;
        DateTime data = new DateTime();
        DataSet linhas;
        DataSet apoio;
        int posto = 0, operador = 0,i=0,estado=0;
        string entidade = "", erro = "";

        //Verifica Picagens a Sincronizar
        query.AppendLine("SELECT PIPicagensInput.* ");
        query.AppendLine("FROM PIPicagensInput ");
        query.AppendLine("WHERE ");
        query.AppendLine("NOT EXISTS( ");
        query.AppendLine("      SELECT PIPicagensSincronismo.picagem ");
        query.AppendLine("      FROM PIPicagensSincronismo ");
        query.AppendLine("      WHERE PIPicagensSincronismo.Picagem = PIPicagensInput.Picagem ");
        query.AppendLine(")");
        linhas = ExecuteQuery(query.ToString(), null);
        if (linhas == null)
            return false;
        //Percorre todas as linhas
        foreach (DataRow lines in linhas.Tables[0].Rows)
        {
            //ATRIBUI VARIAVEIS DE APOIO
            entidade = lines["Picagem"].ToString().Substring(0, 4);
            posto = int.Parse(lines["Picagem"].ToString().Substring(5, 3));
            operador = int.Parse(lines["Picagem"].ToString().Substring(9, 5));
            data = Convert.ToDateTime(lines["Picagem"].ToString().Substring(15, 19));

            //Caso Entidade seja diferente de IPCA não permite registar Picagem
            if (entidade != "IPCA")
                estado = 1;

            //Verifica se o posto existe
            foreach (PostoPicagem post in postoPicagem)
            {
              if (post.Posto != posto && i == int.Parse(postoPicagem.Count.ToString()))
                    estado = 2;
              i++;
            }

            //VERIFICA OP VALIDO
            op = GetOperator(operador.ToString());
            if (op == null)
                estado = 3;

            //VERIFICA FROMATO
            if (data.ToString().Length!=19)
                estado = 4;
        
            //Regista Sincronismo Picagem
            if (estado==0)
            {
                query.Clear();
                query.AppendLine("INSERT INTO PIPicagensSincronismo VALUES (@ID,@Picagem,@Operador,@Data,@Posto,0)");
                paramSQL = new SqlParameter[5];
                paramSQL[0] = new SqlParameter("@ID", lines["ID"].ToString());
                paramSQL[1] = new SqlParameter("@Picagem", lines["Picagem"].ToString());
                paramSQL[2] = new SqlParameter("@Operador", operador);
                paramSQL[3] = new SqlParameter("@Data", data);
                paramSQL[4] = new SqlParameter("@Posto", posto);
                ExecuteQuery(query.ToString(), paramSQL);

                query.Clear();
                query.AppendLine("INSERT INTO PIPicagens VALUES (@Operador,@Data,@Posto,GETDATE(),1,@Periodo,5)");
                paramSQL = new SqlParameter[4];
                paramSQL[0] = new SqlParameter("@Operador", operador);
                paramSQL[1] = new SqlParameter("@Data",data);
                paramSQL[2] = new SqlParameter("@Posto", posto);
                paramSQL[3] = new SqlParameter("@Periodo", Period(operador,data)+1);
                ExecuteQuery(query.ToString(), paramSQL);

                query.Clear();
                query.AppendLine("SELECT Picagem  ");
                query.AppendLine("FROM PIPicagens  ");
                query.AppendLine("  INNER JOIN PIPicagensSincronismo ON  ");                
                query.AppendLine("  PIPicagensSincronismo.Operador = PIPicagens.Operador AND  ");
                query.AppendLine("  PIPicagensSincronismo.Posto = PIPicagens.Posto AND  ");
                query.AppendLine("  PIPicagensSincronismo.Data = PIPicagens.Data  ");
                query.AppendLine("WHERE Sincronizado = 0  ");
                apoio=ExecuteQuery(query.ToString(), null);
                if (apoio == null)
                    return false;

                foreach (DataRow linh in apoio.Tables[0].Rows)
                {
                    query.Clear();
                    paramSQL = new SqlParameter[1];
                    paramSQL[0] = new SqlParameter("@Picagem", linh["Picagem"]);
                    query.AppendLine("UPDATE PIPicagensSincronismo SET Sincronizado=1 ");
                    query.AppendLine("WHERE PICAGEM=@Picagem");
                    ExecuteQuery(query.ToString(), paramSQL);
                }
            }
            else
            {
                if (estado == 1)
                    erro = "ENTITY NOT RECOGNIZED";
                else if (estado == 2)
                    erro = "POST NOT RECOGNIZED";
                else if (estado == 3)
                    erro = "OPERATOR NOT RECOGNIZED";
                else if (estado == 4)
                    erro = "FORMAT NOT RECOGNIZED";
                else
                    erro = "STATE OF ERROR NOT RECOGNIZED";

                query.Clear();
                query.AppendLine("INSERT INTO PSincronismoPicagensFalhas (Picagem,Erro) VALUES (@Picagem,@Erro)");
                paramSQL = new SqlParameter[2];
                paramSQL[0] = new SqlParameter("@Picagem", lines["Picagem"].ToString());
                paramSQL[1] = new SqlParameter("@Erro", erro);
                ExecuteQuery(query.ToString(), paramSQL);
            }
        }

        return true;
    }

    private int Period(int op,DateTime data)
    {
        StringBuilder query = new StringBuilder();
        SqlParameter [] paramSQL;
        DataSet dados;
        query.AppendLine("SELECT COUNT(*)  AS Periodo ");
        query.AppendLine("FROM PIPicagens ");
        query.AppendLine("WHERE Operador = @Operador AND CONVERT(DATE, Data) = @Data");
        paramSQL = new SqlParameter[2];
        paramSQL[0] = new SqlParameter("@Operador", op);
        paramSQL[1] = new SqlParameter("@Data", data.Date);
        dados = ExecuteQuery(query.ToString(),paramSQL);
        if (dados == null)
            return 0;
        return int.Parse(dados.Tables[0].Rows[0].ItemArray[0].ToString());
    }

    private int PeriodNew(int op, DateTime data)
    {
        bool primeiro=false, segundo=false, terceiro=false, quarto=false;
        StringBuilder query = new StringBuilder();
        SqlParameter[] paramSQL;
        DataSet dados;
        query.AppendLine("SELECT Periodo ");
        query.AppendLine("FROM PIPicagens ");
        query.AppendLine("WHERE Operador = @Operador AND CONVERT(DATE, Data) = @Data");
        paramSQL = new SqlParameter[2];
        paramSQL[0] = new SqlParameter("@Operador", op);
        paramSQL[1] = new SqlParameter("@Data", data.Date);
        dados = ExecuteQuery(query.ToString(), paramSQL);
        if (dados == null)
            return 0;
        foreach (DataRow linha in dados.Tables[0].Rows)
        {
            if (int.Parse(linha.ItemArray[0].ToString()) == 1)
                primeiro = true;
            if (int.Parse(linha.ItemArray[0].ToString()) == 2)
                segundo = true;
            if (int.Parse(linha.ItemArray[0].ToString()) == 3)
                terceiro = true;
            if (int.Parse(linha.ItemArray[0].ToString()) == 4)
                quarto = true;
        }

        if (!primeiro)
            return 1;
        if (!segundo)
            return 2;
        if (!terceiro)
            return 3;
        if (!quarto)
            return 4;
        return 0;
    }

    #endregion

}
