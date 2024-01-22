function redirectToStripe(sessionId) {
    var stripe = Stripe("pk_test_51MWKQCGS8y7NleDO6qVbWXKY6E0jDsG5nDmzmJECSM72AHbSuA9MHtnZ4HNYMwIQrvIB1LFD7f8iWwfY8q2pyf8B00TXwTnUKP");
    stripe.redirectToCheckout({ sessionId: sessionId });
}