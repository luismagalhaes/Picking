using System;
using System.Runtime.Serialization;

[DataContract]
public class DataHora
{
    DateTime data;

    public DataHora(DateTime dataC)
    {
        data = dataC;
    }

    [DataMember]
    public DateTime Data
    {
        get { return data; }
        set { data = value; }
    }
}

[DataContract]
public class PicagemDetalhes
{
    int operador,estado;
    string posto, nome, turno;
    DateTime dia, hora;

    public PicagemDetalhes(int operador, string nome,string turno, string posto, DateTime dia, DateTime hora, int estado)
    {
        this.operador = operador;
        this.nome = nome;
        this.turno = turno;
        this.posto = posto;
        this.dia = dia;
        this.hora = hora;
        this.estado = estado;
    }

    [DataMember]
    public int Operador
    {
        get { return operador; }
        set { operador = value; }
    }

    [DataMember]
    public string Posto
    {
        get { return posto; }
        set { posto = value; }
    }

    [DataMember]
    public string Nome
    {
        get { return nome; }
        set { nome = value; }
    }

    [DataMember]
    public string Turno
    {
        get { return turno; }
        set { turno = value; }
    }

    [DataMember]
    public int Estado
    {
        get { return estado; }
        set { estado = value; }
    }

    [DataMember]
    public DateTime Hora
    {
        get { return hora; }
        set { hora = value; }
    }

    [DataMember]
    public DateTime Dia
    {
        get { return dia; }
        set { dia = value; }
    }
}

[DataContract]
public class PostoPicagem
{
    int posto;
    string nome;

    public PostoPicagem(int post, string name)
    {
        posto = post;
        nome = name;
    }

    [DataMember]
    public int Posto
    {
        get { return posto; }
        set { posto = value; }
    }

    [DataMember]
    public string Nome
    {
        get { return nome; }
        set { nome = value; }
    }
}

[DataContract]
public class Operador
{
    int numero;
    string nome,turno;
   
    public Operador(int numero, string nome, string turno)
    {
        this.numero = numero;
        this.nome = nome;
        this.turno = turno;
    }

    [DataMember]
    public int Numero
    {
        get { return numero; }
        set { numero = value; }
    }

    [DataMember]
    public string Nome
    {
        get { return nome; }
        set { nome = value; }
    }

    [DataMember]
    public string Turno
    {
        get { return turno; }
        set { turno = value; }
    }
}

[DataContract]
public class AccaoSobrePicagem
{
    int numero;
    string nome;

    public AccaoSobrePicagem(int number, string name)
    {
        numero = number;
        nome = name;
    }

    [DataMember]
    public int Numero
    {
        get { return numero; }
        set { numero = value; }
    }

    [DataMember]
    public string Nome
    {
        get { return nome; }
        set { nome = value; }
    }
}

[DataContract]
public class IsAlive
{
    bool estado;

    public IsAlive(bool estad)
    {
        this.estado = estad;
    }

    [DataMember]
    public bool Estado
    {
        get { return estado; }
        set { estado = value; }
    }
}

[DataContract]
public class TurnoTempos
{
    string nome;
    DateTime dataPica, tolerancia, normalIni, normalFim, normalDuracao;

    public TurnoTempos(string nome,DateTime dataPica, DateTime tolerancia, DateTime normalIni, DateTime normalFim, DateTime normalDuracao)
    {
        this.nome = nome;
        this.dataPica = dataPica;
        this.tolerancia = tolerancia;
        this.normalIni = normalIni;
        this.normalFim = normalFim;
        this.normalDuracao = normalDuracao;
    }

    [DataMember]
    public string Nome
    {
        get { return nome; }
        set { nome = value; }
    }

    [DataMember]
    public DateTime DataPicagem
    {
        get { return dataPica; }
        set { dataPica = value; }
    }
    [DataMember]
    public DateTime Tolerancia
    {
        get { return tolerancia; }
        set { tolerancia = value; }
    }
    [DataMember]
    public DateTime NormalIni
    {
        get { return normalIni; }
        set { normalIni = value; }
    }
    [DataMember]
    public DateTime NormalFim
    {
        get { return normalFim; }
        set { normalFim = value; }
    }
    [DataMember]
    public DateTime TempoAlmoco
    {
        get { return normalDuracao; }
        set { normalDuracao = value; }
    }

}

[DataContract]
public class PicagemEstado
{
    int numero;
    string descr;

    public PicagemEstado(int numero, string descr)
    {
        this.numero = numero;
        this.descr = descr;
    }

    [DataMember]
    public int Numero
    {
        get { return numero; }
        set { numero = value; }
    }

    [DataMember]
    public string Descr
    {
        get { return descr; }
        set { descr = value; }
    }
}


[DataContract]
public class Pick
{
    string descr;

    public Pick(string descr)
    {
        this.descr = descr;
    }

    [DataMember]
    public string Descr
    {
        get { return descr; }
        set { descr = value; }
    }
}


[DataContract]
public class PicagemJustificacao
{
    int op, opJust;
    DateTime dataOld, dataNew,dataCria;
    string obs, causa,nomeJust;
    bool novo;

    public PicagemJustificacao(int op, int opJust,string nomeJust,DateTime dataOld,DateTime dataNew,string obs,string causa,bool novo,DateTime dataCria)
    {
        this.op = op;
        this.opJust = opJust;
        this.dataOld = dataOld;
        this.dataNew = dataNew;
        this.obs = obs;
        this.causa = causa;
        this.novo = novo;
        this.nomeJust = nomeJust;
        this.dataCria = dataCria;
    }

    [DataMember]
    public int Op
    {
        get { return op; }
        set { op = value; }
    }

    [DataMember]
    public int OpJust
    {
        get { return opJust; }
        set { opJust = value; }
    }

    [DataMember]
    public DateTime DataOld
    {
        get { return dataOld; }
        set { dataOld = value; }
    }

    [DataMember]
    public DateTime DataCria
    {
        get { return dataCria; }
        set { dataCria = value; }
    }

    [DataMember]
    public DateTime DataNew
    {
        get { return dataNew; }
        set { dataNew = value; }
    }

    [DataMember]
    public string Obs
    {
        get { return obs; }
        set { obs = value; }
    }

    [DataMember]
    public string NomeJust
    {
        get { return nomeJust; }
        set { nomeJust = value; }
    }

    [DataMember]
    public string Causa
    {
        get { return causa; }
        set { causa = value; }
    }

    [DataMember]
    public bool Novo
    {
        get { return novo; }
        set { novo = value; }
    }
}
