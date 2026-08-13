import assert from "node:assert/strict";

const baseUrl = (process.env.WORXFORGE_WEB_URL ?? "http://127.0.0.1:5173").replace(/\/$/, "");

async function request(path, init, expectedStatuses) {
  const response = await fetch(`${baseUrl}${path}`, init);
  const text = await response.text();

  if (!expectedStatuses.includes(response.status)) {
    throw new Error(`${init?.method ?? "GET"} ${path} returned HTTP ${response.status}: ${text || "<empty response>"}`);
  }

  if (!text) {
    return { response, body: undefined };
  }

  try {
    return { response, body: JSON.parse(text) };
  } catch {
    throw new Error(`${init?.method ?? "GET"} ${path} returned invalid JSON: ${text.slice(0, 300)}`);
  }
}

const login = await request(
  "/api/authentication/debug-login",
  { method: "POST", headers: { Accept: "application/json" } },
  [200]
);
const accessToken = login.body?.accessToken;
assert.equal(typeof accessToken, "string", "Debug login did not return accessToken.");

const authenticatedHeaders = {
  Accept: "application/json",
  Authorization: `Bearer ${accessToken}`
};

const [assetTypes, domains, useCases, tools] = await Promise.all([
  request("/api/assets/types", undefined, [200]),
  request("/api/assets/domains", undefined, [200]),
  request("/api/assets/use-cases", undefined, [200]),
  request("/api/assets/tools", undefined, [200])
]);

assert.ok(assetTypes.body?.length, "No asset types are available.");
assert.ok(domains.body?.length, "No domains are available.");
assert.ok(useCases.body?.length, "No use cases are available.");
assert.ok(tools.body?.length, "No compatible tools are available.");

const markdownContent = "# Publish verification\n\nCreated by the publish lifecycle smoke test.";
const formData = new FormData();
formData.append("file", new Blob([markdownContent], { type: "text/markdown" }), "publish-verification.md");
const upload = await request(
  "/api/assets/upload-markdown",
  {
    method: "POST",
    headers: authenticatedHeaders,
    body: formData
  },
  [200]
);
assert.equal(upload.body?.markdownContent, markdownContent, "Markdown upload content did not round-trip.");

const title = `Publish verification ${Date.now()}`;
const createBody = {
  title,
  description: "End-to-end publish verification",
  markdownContent: upload.body.markdownContent,
  assetTypeId: assetTypes.body[0].id,
  domainId: domains.body[0].id,
  useCaseId: useCases.body[0].id,
  toolIds: [tools.body[0].id],
  tags: ["verification"]
};

let assetId;
const result = {
  loginStatus: login.response.status,
  uploadStatus: upload.response.status
};

try {
  const created = await request(
    "/api/assets",
    {
      method: "POST",
      headers: { ...authenticatedHeaders, "Content-Type": "application/json" },
      body: JSON.stringify(createBody)
    },
    [201]
  );

  assetId = created.body?.assetId;
  assert.equal(typeof assetId, "string", "Publish response did not include assetId.");
  result.createStatus = created.response.status;
  result.assetId = assetId;

  const details = await request(`/api/assets/${assetId}`, { headers: authenticatedHeaders }, [200]);
  assert.equal(details.body?.title, title, "Created asset could not be read back correctly.");
  result.readStatus = details.response.status;

  const updatedTitle = `${title} updated`;
  const updated = await request(
    `/api/assets/${assetId}`,
    {
      method: "PUT",
      headers: { ...authenticatedHeaders, "Content-Type": "application/json" },
      body: JSON.stringify({
        ...createBody,
        title: updatedTitle,
        description: "Updated end-to-end publish verification",
        tags: ["verification", "updated"]
      })
    },
    [200]
  );
  result.updateStatus = updated.response.status;

  const updatedDetails = await request(`/api/assets/${assetId}`, { headers: authenticatedHeaders }, [200]);
  assert.equal(updatedDetails.body?.title, updatedTitle, "Updated asset could not be read back correctly.");
  result.updatedReadStatus = updatedDetails.response.status;
} finally {
  if (assetId) {
    const deleted = await request(
      `/api/assets/${assetId}`,
      { method: "DELETE", headers: authenticatedHeaders },
      [204]
    );
    result.deleteStatus = deleted.response.status;
  }
}

console.log(JSON.stringify(result, null, 2));