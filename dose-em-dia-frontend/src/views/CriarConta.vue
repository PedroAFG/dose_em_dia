<template>
  <div class="container py-5">
    <h1 class="text-orange fw-bold">Dose em dia</h1>
    <h4 class="mt-2">Crie sua conta</h4>
    <p class="text-muted">Mantenha sua vacinação em dia!</p>

    <form @submit.prevent="criarConta" class="bg-white p-4 rounded shadow mt-4">
      <div class="row g-3">
        <!-- Coluna 1 -->
        <div class="col-md-6">
          <label class="form-label">Nome completo*</label>
          <input type="text" class="form-control" v-model="form.nome" required />

          <label class="form-label mt-3">E-mail*</label>
          <input type="email" class="form-control" v-model="form.email" required />

          <label class="form-label mt-3">Telefone*</label>
          <input type="text" class="form-control" v-mask="'(##) #####-####'" v-model="form.telefone" required />

          <label class="form-label mt-3">CPF*</label>
          <input type="text" class="form-control" v-mask="'###.###.###-##'" v-model="form.cpf" required />

          <label class="form-label mt-3">Data de nascimento*</label>
          <input type="date" class="form-control" v-model="form.dataNascimento" required />
        </div>

        <!-- Coluna 2 -->
        <div class="col-md-6">
          <label class="form-label">CEP*</label>
          <input type="text" class="form-control" v-mask="'#####-###'" v-model="form.cep" @blur="buscarCep" required />

          <label class="form-label mt-3">Endereço*</label>
          <input type="text" class="form-control" v-model="form.endereco.logradouro" required />

          <label class="form-label mt-3">Estado*</label>
          <input type="text" class="form-control" v-model="form.endereco.estado" required />

          <label class="form-label mt-3">País*</label>
          <input type="text" class="form-control" v-model="form.endereco.pais" required />

          <label class="form-label mt-3">Cidade*</label>
          <input type="text" class="form-control" v-model="form.endereco.cidade" required />

          <label class="form-label mt-3">Bairro*</label>
          <input type="text" class="form-control" v-model="form.endereco.bairro" required />

          <label class="form-label mt-3">Senha*</label>
          <input type="password" class="form-control" v-model="form.senha" required />

          <label class="form-label mt-3">Confirme sua senha*</label>
          <input type="password" class="form-control" v-model="form.confirmarSenha" required />
        </div>
      </div>

      <div class="d-flex justify-content-end mt-4">
        <button type="button" class="btn btn-outline-orange me-2" @click="$router.push('/')">
          Entrar
        </button>
        <button type="submit" class="btn btn-orange">Criar</button>
      </div>
    </form>

    <small class="text-muted mt-2 d-block">*Informações obrigatórias</small>
  </div>
  <!-- Modal de carregamento -->
  <div v-if="carregando" class="modal-loading">
    <div class="modal-content text-white text-center p-4">
      <h4>Você está quase lá!</h4>
      <p>Criando conta...</p>
      <button class="btn btn-light mt-3" @click="carregando = false">Cancelar</button>
    </div>
  </div>
</template>

<script>
import axios from "axios";
import { mask } from "vue-the-mask";

export default {
  name: "CriarConta",
  directives: { mask },
  data() {
    return {
      carregando: false,

      form: {
        nome: "",
        email: "",
        telefone: "",
        cpf: "",
        dataNascimento: "",
        cep: "",
        senha: "",
        confirmarSenha: "",
        endereco: {
          logradouro: "",
          bairro: "",
          cidade: "",
          estado: "",
          cep: "",
          pais: "",
        },
      },
    };
  },
  methods: {
    async buscarCep() {
      const cepLimpo = this.form.cep.replace(/\D/g, "");
      if (cepLimpo.length !== 8) return;

      try {
        const response = await axios.get(
          `https://viacep.com.br/ws/${cepLimpo}/json/`
        );
        const data = response.data;
        if (data.erro) throw new Error("CEP inválido");

        this.form.endereco.logradouro = data.logradouro;
        this.form.endereco.bairro = data.bairro;
        this.form.endereco.cidade = data.localidade;
        this.form.endereco.estado = data.uf; //correção do BUG que ocorria ao tentar salvar um estado como "Sao paulo", limite de caracteres do banco era em 2
      } catch (error) {
        alert("CEP inválido ou erro ao buscar endereço.");
      }
    },
    async criarConta() {
      if (this.form.senha !== this.form.confirmarSenha) {
        alert("As senhas não coincidem.");
        return;
      }

      const payload = {
        nome: this.form.nome,
        email: this.form.email,
        telefone: this.form.telefone,
        cpf: this.form.cpf,
        dataNascimento: this.form.dataNascimento,
        senha: this.form.senha,
        endereco: {
          logradouro: this.form.endereco.logradouro,
          bairro: this.form.endereco.bairro,
          cidade: this.form.endereco.cidade,
          estado: this.form.endereco.estado,
          cep: this.form.endereco.cep,
          pais: this.form.endereco.pais
        },
      };

      try {
        this.carregando = true;
        const response = await axios.post("http://localhost:5054/api/usuario/criar", payload);
        localStorage.setItem("usuarioNome", response.data.nome);
        console.log("Usuário criado:", response.data);
        alert("Conta criada com sucesso!");
        this.$router.push("/");
      } catch (err) {
        // 👇 Novo tratamento de erro com mais clareza
        if (err.response && err.response.data) {
          console.error("Erro do servidor:", err.response.data);
          const erroMensagem = typeof err.response.data === 'string'
            ? err.response.data
            : JSON.stringify(err.response.data);
          alert("Erro ao criar conta: " + erroMensagem);
        } else {
          console.error("Erro inesperado:", err);
          alert("Erro desconhecido ao criar conta.");
        }
      } finally {
        this.carregando = false;
      }

    },
  },
};
</script>

<style scoped>
.text-orange {
  color: #f46c20;
}

.btn-orange {
  background-color: #f46c20;
  color: white;
  border: none;
}

.btn-orange:hover {
  background-color: #d85c1a;
}

.btn-outline-orange {
  border: 1px solid #f46c20;
  color: #f46c20;
}

.btn-outline-orange:hover {
  background-color: #f46c20;
  color: white;
}

/* estilização do modal */
.modal-loading {
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background-color: rgba(0, 0, 0, 0.5);
  /* fundo escuro translúcido */
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 9999;
}

.modal-content {
  background-color: #f46c20;
  border-radius: 20px;
  padding: 40px;
  width: 300px;
  box-shadow: 0 10px 25px rgba(0, 0, 0, 0.3);
}
</style>
