export const fetchResources = async () => {
    const response = await fetch('api/resources');
    const result = await response.json();
    if (response.status !== 200) {
        throw Error(result.detail);
    }
    return result;
}

export const fetchRecommendations = async (resources) => {
    const response = await fetch('api/resources/recommendations', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ resources: resources })
    });
    const result = await response.json();
    if (response.status !== 200) {
        throw Error(result.detail);
    }
    return result;
}

export const fetchBenchmarkControls = async () => {
    const response = await fetch('/api/benchmark/controls');
    const result = await response.json();
    if (response.status !== 200) {
        throw Error(result.detail);
    }
    return result;
}
