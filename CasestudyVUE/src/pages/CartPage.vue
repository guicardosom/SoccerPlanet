<template>
  <div class="text-center">
    <div class="text-h4 q-mt-md text-secondary">Cart Contents</div>
    <q-avatar class="q-mt-md" size="xl" square>
      <img :src="`img/cart.jpg`" />
    </q-avatar>
    <div class="text-h6 text-positive">{{ state.status }}</div>
  </div>

  <div v-if="state.cart.length > 0">
    <q-scroll-area style="height: 55vh">
      <q-card class="q-ma-md">
        <q-item class="text-bold">
          <q-item-section style="flex-grow: 3"> Name </q-item-section>
          <q-item-section style="flex-grow: 1; align-items: center">
            Qty
          </q-item-section>
          <q-item-section style="flex-grow: 1"> MSRP </q-item-section>
          <q-item-section style="flex-grow: 1"> Extended </q-item-section>
        </q-item>

        <q-list separator>
          <q-item v-for="item in state.cart" :key="item.id">
            <q-item-section style="flex-grow: 3">
              {{ item.product.productName }}
            </q-item-section>
            <q-item-section style="flex-grow: 1; align-items: center">
              {{ item.qty }}
            </q-item-section>
            <q-item-section style="flex-grow: 1">
              {{ formatCurrency(item.product.msrp) }}
            </q-item-section>
            <q-item-section style="flex-grow: 1; align-items: end">
              {{ formatCurrency(item.product.msrp * item.qty) }}
            </q-item-section>
          </q-item>

          <q-item>
            <q-item-section class="text-bold">Sub: </q-item-section>
            <q-item-section style="align-items: end">{{
              formatCurrency(state.subtotal)
            }}</q-item-section>
          </q-item>

          <q-item>
            <q-item-section class="text-bold">Tax(13%): </q-item-section>
            <q-item-section style="align-items: end">{{
              formatCurrency(state.taxes)
            }}</q-item-section>
          </q-item>

          <q-item>
            <q-item-section class="text-bold">Total: </q-item-section>
            <q-item-section style="align-items: end">{{
              formatCurrency(state.total)
            }}</q-item-section>
          </q-item>
        </q-list>
      </q-card>
    </q-scroll-area>

    <div class="text-center">
      <q-btn
        icon="shopping_cart"
        class="q-mr-sm"
        color="primary"
        label="Checkout"
        :disable="state.cart.length < 1"
        @click="saveOrder()"
      />
      <q-btn
        icon="delete"
        color="primary"
        label="Clear Cart"
        @click="clearCart()"
        style="max-width: 40vw"
      />
    </div>
  </div>
</template>

<script>
import { reactive, onMounted } from "vue";
import { poster } from "../utils/apiutil";
import { formatCurrency } from "../utils/formatutils";

export default {
  setup() {
    onMounted(() => {
      loadCart();

      state.cart.forEach((item) => {
        state.subtotal += item.product.msrp * item.qty;
      });

      state.taxes = (state.subtotal * 13) / 100.0;
      state.total = state.subtotal + state.taxes;
    });

    let state = reactive({
      status: "",
      cart: [],
      subtotal: 0,
      taxes: 0,
      total: 0,
    });

    const loadCart = () => {
      if (sessionStorage.getItem("cart")) {
        state.cart = JSON.parse(sessionStorage.getItem("cart"));
      }
    };

    const clearCart = () => {
      sessionStorage.removeItem("cart");
      state.cart = [];
      state.status = "cart emptied";
    };

    const saveOrder = async () => {
      let customer = JSON.parse(sessionStorage.getItem("customer"));
      let cart = JSON.parse(sessionStorage.getItem("cart"));
      try {
        state.status = "sending cart info to server";
        let orderHelper = { email: customer.email, selections: cart };
        let payload = await poster("Order", orderHelper);
        if (payload.indexOf("not") > 0) {
          state.status = payload;
        } else {
          clearCart();
          state.status = payload;
        }
      } catch (err) {
        console.log(err);
        state.status = `Error add cart: ${err}`;
      }
    };

    return {
      state,
      clearCart,
      saveOrder,
      formatCurrency,
    };
  },
};
</script>
