<template>
  <div id="app">
    <!-- Navbar ou Header se quiser -->
    <VacinasObrigatorias />
    <BotaoAcessibilidade />
  </div>
</template>

<script>
import BotaoAcessibilidade from './components/acessibilidade/BotaoAcessibilidade.vue';
import VacinasObrigatorias from './components/VacinasObrigatorias.vue';

export default {
  components: {
    BotaoAcessibilidade,
    VacinasObrigatorias
  },
};
</script>
